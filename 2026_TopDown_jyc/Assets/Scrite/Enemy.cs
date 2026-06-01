using UnityEngine;

public class EnemyTraceController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float raycastDistance = 2f;
    public float traceDistance = 5f;

    private Transform player;
    private bool isFacingRight = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("플레이어 인식 성공 " + playerObj.name);
        }
        else
        {
            Debug.LogError("플레이어를 못 찾았습니다 태그를 확인하세요.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 1. 플레이어와의 방향 및 거리 계산
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // 추격 거리 밖이면 아무것도 안 함
        if (distance > traceDistance) return;

        Vector2 dirNormalized = direction.normalized;

        // 2. 장애물 감지
        // Obstacle 태그가 달린 물체를 감지
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirNormalized, raycastDistance);
        Debug.DrawRay(transform.position, dirNormalized * raycastDistance, Color.red);

        Vector2 finalDirection;

        // 장애물 충돌 여부 확인
        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
        {
            // 장애물이 있으면 오른쪽으로 90도 꺾어서 우회
            finalDirection = Quaternion.Euler(0, 0, -90f) * dirNormalized;
        }
        else
        {
            // 장애물이 없으면 플레이어에게 직진
            finalDirection = dirNormalized;
        }

        // 3. 바라보는 방향 전환 (Flip) 호출
        Flip(finalDirection.x);

        // 4. 이동 처리 (Space.World를 넣어야 계산된 월드 좌표대로 정확히 움직임)
        transform.Translate(finalDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    // 캐릭터의 좌우 이미지를 반전시키는 함수
    private void Flip(float horizontalDir)
    {
        // 미세하게 움직일 때는 반전하지 않음 (떨림 방지)
        if (Mathf.Abs(horizontalDir) < 0.01f) return;

        // 진행 방향과 현재 바라보는 방향이 다를 때만 반전
        if ((horizontalDir > 0 && !isFacingRight) || (horizontalDir < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;

            // LocalScale의 X값을 반전시켜 자식 오브젝트까지 한꺼번에 뒤집음
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }
}