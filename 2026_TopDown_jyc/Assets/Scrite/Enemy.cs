using UnityEngine;

public class EnemyTraceController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float raycastDistance = 2f;
    public float traceDistance = 5f;

    [Header("공격 세팅")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    private Animator animator;
    private Transform player;

    private void Start()
    {
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("플레이어 인식 성공 " + playerObj.name);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // -------------------------------------------------------------
        // ★ 추가: 공격 애니메이션 재생 중이면 즉시 리턴하여 이동/연산 멈춤!
        // (애니메이터 창의 공격 노드 이름인 "Enemy_Attack"과 대소문자가 정확히 같아야 합니다)
        // -------------------------------------------------------------
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Enemy_Attack"))
        {
            return;
        }

        // 1. 플레이어와의 방향 및 거리 계산
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > traceDistance)
        {
            // 추격 범위 밖으로 나가면 속도를 0으로 만들어 멈춤 애니메이션 재생
            if (animator != null) animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        Vector2 dirNormalized = direction.normalized;

        // 2. 장애물 감지 및 우회 로직
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirNormalized, raycastDistance);
        Debug.DrawRay(transform.position, dirNormalized * raycastDistance, Color.red);

        Vector2 finalDirection;
        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
        {
            finalDirection = Quaternion.Euler(0, 0, -90f) * dirNormalized;
        }
        else
        {
            finalDirection = dirNormalized;
        }

        // 3. 이동 처리
        transform.Translate(finalDirection * moveSpeed * Time.deltaTime, Space.World);

        // 4. 애니메이터 파라미터 제어 (상하좌우 완벽 대응)
        if (animator != null)
        {
            if (finalDirection.sqrMagnitude > 0.01f)
            {
                // 움직일 때는 실시간 방향X, 방향Y 값을 찔러줍니다.
                animator.SetFloat("DirX", finalDirection.x);
                animator.SetFloat("DirY", finalDirection.y);
                animator.SetFloat("MoveSpeed", finalDirection.magnitude);
            }
            else
            {
                // 완전히 멈췄을 때는 MoveSpeed만 0으로 만들어 줍니다. (마지막 바라보던 방향 유지)
                animator.SetFloat("MoveSpeed", 0f);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Health playerHealth = collision.gameObject.GetComponent<Health>();

                if (playerHealth != null)
                {
                    // 1. 플레이어에게 대미지 주기
                    playerHealth.TakeDamage(attackDamage);
                    lastAttackTime = Time.time;
                    Debug.Log($"[몬스터 공격] 플레이어에게 {attackDamage} 데미지를 입혔습니다!");

                    // ★ 2. 여기에 대망의 공격 애니메이션 발동 코드 추가!
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }
                }
            }
        }
    }
}