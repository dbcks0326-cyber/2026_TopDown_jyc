using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수 추가

[Serializable]
public class PlayerData
{
    public List<string> collectedItems = new List<string>();

    public int coin = 0;
    public int stage = 1;

    public float volume = 1f;
    public bool BGM = true;
    public string currentJob = "Citizen";

    // -------------------------------------------------------------
    // ★ 추가: Health.cs에서 참조할 플레이어의 최대 체력과 현재 체력
    // -------------------------------------------------------------
    public float maxHp = 100f;
    public float currentHp = 100f;
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;
    public PlayerData playerData;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerData = LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------
    // ★ 추가: 새로운 스테이지(씬)가 켜질 때마다 자동으로 실행되는 유니티 시스템
    // -------------------------------------------------------------
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드되었을 때 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 💡 [기획 조건 설정]: 원하는 스테이지 씬 이름을 여기에 적어줍니다.
        // 예를 들어 게임 시작 스테이지인 "Stage_0"이나 "Main" 마을 씬으로 가면 풀피가 되도록 세팅합니다.
        if (scene.name == "Stage_1" || scene.name == "Main")
        {
            playerData.currentHp = playerData.maxHp; // 체력을 100(최대치)으로 회복!

            // 데이터가 바뀐 상태를 하드디스크에 안전하게 바로 저장
            SaveData(playerData);

            Debug.Log($"[{scene.name}] 스테이지 진입: 플레이어 체력이 만땅({playerData.maxHp})으로 회복되었습니다!");
        }
    }
    // -------------------------------------------------------------

    public void SaveData(PlayerData playerData)
    {
        string filePath = Application.persistentDataPath + "/player_data.json";
        string json = JsonUtility.ToJson(playerData, true);
        System.IO.File.WriteAllText(filePath, json);
    }

    public PlayerData LoadData()
    {
        string filePath = Application.persistentDataPath + "/player_data.json";
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
            return playerData;
        }
        else
        {
            return new PlayerData();
        }
    }
}