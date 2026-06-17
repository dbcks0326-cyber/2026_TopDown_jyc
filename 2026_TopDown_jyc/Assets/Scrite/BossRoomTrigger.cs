using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    private bool hasTriggered = false; // 체력바가 중복으로 계속 켜지는 걸 방지하는 안전장치

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 이미 들어왔었거나, 부딪힌 오브젝트가 플레이어가 아니라면 무시합니다.
        if (hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            // 2. 보스 체력바 UI를 켭니다!
            if (BossHPBar.Instance != null)
            {
                BossHPBar.Instance.ShowHPBar();
                hasTriggered = true; // 한 번 켜졌으므로 다시 실행 안 되게 막음

                Debug.Log("🏁 플레이어 보스 구역 진입! 보스 HP바 활성화");
            }
        }
    }
}