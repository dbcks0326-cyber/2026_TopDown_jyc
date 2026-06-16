using UnityEngine;

public class EnemyTraceController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float raycastDistance = 2f;
    public float traceDistance = 1f;

    [Header("공격 세팅")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    private Animator animator;
    private Transform player;
    private EnemyHealth enemyHealth;

    // -------------------------------------------------------------
    // ★ [추가]: 대쉬 공격 중일 때 이동을 멈추기 위해 컴포넌트를 가져옵니다.
    // -------------------------------------------------------------
    private EnemyAttackPattern attackPattern;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        // ★ 대쉬 공격 스크립트를 내 몸에서 미리 찾아둡니다.
        attackPattern = GetComponent<EnemyAttackPattern>();

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
        // ★ [핵심 추가]: 지금 대쉬 공격(차징/돌진 포함) 중이라면
        // 모든 AI 추격 이동 연산을 즉시 멈추고 리턴시킵니다!
        // -------------------------------------------------------------
        if (attackPattern != null && attackPattern.isAttacking)
        {
            // 애니메이션 속도 파라미터를 0으로 만들어 걷는 모션을 멈춥니다.
            if (animator != null) animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Enemy_Attack"))
        {
            return;
        }

        if (enemyHealth != null && enemyHealth.enabled == false)
        {
            return;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            return;
        }

        // 1. 플레이어와의 방향 및 거리 계산
        // 1. 플레이어와의 방향 및 거리 계산
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > traceDistance)
        {
            if (animator != null)
            {
                // 1. 모든 이동 파라미터를 0으로 만들어 애니메이션 보행을 중단시킵니다.
                animator.SetFloat("MoveSpeed", 0f);
                animator.SetFloat("DirX", 0f);
                animator.SetFloat("DirY", 0f);

                // 2. [변경]: 이름을 직접 적는 대신, 기본 상태(Entry)로 강제 업데이트 처리를 요청합니다.
                // 이 방식은 컨트롤러 레이어의 첫 번째 기본 상태를 호출하므로 오타 에러가 발생하지 않습니다.
                animator.Rebind();
            }
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

        // 4. 애니메이터 파라미터 제어
        if (animator != null)
        {
            if (finalDirection.sqrMagnitude > 0.01f)
            {
                animator.SetFloat("DirX", finalDirection.x);
                animator.SetFloat("DirY", finalDirection.y);
                animator.SetFloat("MoveSpeed", finalDirection.magnitude);
            }
            else
            {
                animator.SetFloat("MoveSpeed", 0f);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // -------------------------------------------------------------
            // ★ [추가]: 지금 대쉬 패턴 중(준비 및 돌진)이라면 
            // 몸에 닿아도 틱당 들어가는 일반 '기본 공격'은 완전히 무시합니다!
            // -------------------------------------------------------------
            if (attackPattern != null && attackPattern.isAttacking)
            {
                return;
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Health playerHealth = collision.gameObject.GetComponent<Health>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    lastAttackTime = Time.time;
                    Debug.Log($"[몬스터 공격] 플레이어에게 {attackDamage} 데미지를 입혔습니다!");

                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }
                }
            }
        }
    }
}