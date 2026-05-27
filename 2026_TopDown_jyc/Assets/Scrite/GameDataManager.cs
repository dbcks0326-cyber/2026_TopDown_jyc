using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEditor.Progress;




[Serializable]

public class PlayerData
{

    public List<string> collectedItems = new List<string>();

    public int stage = 1;

    public float volume = 1f;
    public bool BGM = true;
    
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

    public void SaveData(PlayerData playerData)
    {
        string filePath = Application.persistentDataPath + "/player_data.json";
        string json = JsonUtility.ToJson(playerData, true);
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log("게임 데이터 저장됨: " + json);

        
    }

    public PlayerData LoadData()
    {
        string filePath = Application.persistentDataPath + "/player_data.json";
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("게임 데이터 로드됨: " + json);
            return playerData;
        }
        else
        {
            Debug.LogWarning("저장된 게임 데이터가 없습니다.");
            return new PlayerData();
        }
    }
}
