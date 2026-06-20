using UnityEngine;

public class SlowTile : MonoBehaviour
{
    [Header("감속 설정")]
    [Tooltip("이 발판 위에서 플레이어 속도를 몇 %로 줄일지 설정 (0.4 = 원래 속도의 40%)")]
    [SerializeField] private float slowRate = 0.4f;

    private float originalSpeed; // 플레이어의 원래 속도를 기억할 변수

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 발판 안으로 플레이어가 쏙 들어왔을 때 실행됩니다.
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // 1. 원래 속도를 미리 백업해 둡니다.
                originalSpeed = player.moveSpeed;

                // 2. 플레이어의 속도를 원하는 비율만큼 깎습니다.
                player.moveSpeed = originalSpeed * slowRate;

                Debug.Log($"🏃‍♂️ 플레이어 감속 시작! 현재 속도: {player.moveSpeed}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 플레이어가 발판 밖으로 완전히 벗어났을 때 실행됩니다.
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // 3. 맵을 벗어났으니 다시 원래 백업해 둔 속도로 원상복구 시킵니다.
                player.moveSpeed = originalSpeed;

                Debug.Log($"🚀 플레이어 감속 해제! 원상복구 속도: {player.moveSpeed}");
            }
        }
    }
}