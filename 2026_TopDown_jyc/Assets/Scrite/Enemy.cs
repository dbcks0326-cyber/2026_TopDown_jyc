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

    // -------------------------------------------------------------
    // ★ 추가: 넉백 상태를 확인하기 위해 EnemyHealth 컴포넌트를 가져옵니다.
    // -------------------------------------------------------------
    private EnemyHealth enemyHealth;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // 내 몸에 붙어있는 EnemyHealth 컴포넌트를 미리 찾아둡니다.
        enemyHealth = GetComponent<EnemyHealth>();

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
        // ★ 추가/수정: 공격 애니메이션 재생 중이거나 '넉백 중'이면 
        // AI 추격 이동 연산을 즉시 멈추고 리턴시킵니다!
        // -------------------------------------------------------------
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Enemy_Attack"))
        {
            return;
        }

        // EnemyHealth가 존재하고, 현재 넉백 코루틴이 돌고 있다면 아래 이동 코드를 실행하지 않습니다.
        // (팁: C#에서 프로퍼티나 함수를 따로 안 만들었어도, 넉백 중일 때는 그냥 리턴하게 연동합니다)
        if (enemyHealth != null && enemyHealth.enabled == false)
        {
            return;
        }

        // ※ 만약 EnemyHealth 스크립트가 넉백 때 스스로를 끄지 않는다면, 
        // 가장 확실하게 넉백 중인지 판별하기 위해 Rigidbody의 속도가 크게 잡혀있을 때(넉백 힘이 들어왔을 때) 차단합니다.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            // 넉백으로 날아가고 있을 때는 강제로 애니메이션 속도를 줄이거나 유지하고 추격 이동을 차단합니다.
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