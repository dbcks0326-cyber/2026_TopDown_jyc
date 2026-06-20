using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelObject : MonoBehaviour
{
    [Header("이동할 씬(스테이지) 이름")]
    public string nextLevel;

    [Header("스테이지 2 진입 제한 설정")]
    [Tooltip("메인 마을에 배치된 스테이지 2 포탈만 체크(true)하세요.")]
    [SerializeField] private bool isStage2Portal = false;

    [Tooltip("스테이지 1 슬라임 보스가 주는 스크립터블 오브젝트(itemso)의 정확한 Item Name을 적으세요.")]
    [SerializeField] private string requiredItemName = "SlimeBossKey";

    public void MoveToNextLevel()
    {
        // 1. 만약 스테이지 2로 진입하려는 포탈이라면 아이템 검사를 진행합니다.
        if (isStage2Portal)
        {
            // 데이터 매니저가 무사히 존재하고, 그 안의 리스트에 필요한 아이템 명칭이 포함되어 있는지 검사
            if (GameDataManager.Instance != null &&
                GameDataManager.Instance.playerData.collectedItems.Contains(requiredItemName))
            {
                Debug.Log($"🔑 보스 전리품 [{requiredItemName}] 확인 완료! 스테이지 2 진입 성공.");
                SceneManager.LoadScene(nextLevel);
            }
            else
            {
                Debug.LogWarning($"❌ [{requiredItemName}] 아이템이 없어 스테이지 2에 들어갈 수 없습니다! 스테이지 1 보스를 먼저 처치하세요.");
                // 이곳에 "보스 아이템이 필요합니다" 같은 UI 팝업 문구를 띄우는 코드를 추가하면 기획 완성도가 높아집니다.
            }
        }
        else
        {
            // 2. 메인 마을, 스테이지 1 등 제한이 없는 씬 이동은 프리패스로 그냥 보냅니다.
            SceneManager.LoadScene(nextLevel);
        }
    }
}