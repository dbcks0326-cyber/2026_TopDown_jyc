using System.Collections;
using UnityEngine;

public class SlimeBossController : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private float contactDamage = 15f;

    [Header("추격 설정")]
    [SerializeField] private float traceDistance = 10f;
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private float defaultJumpInterval = 1.5f;

    [Header("순찰(기어 다니기) 설정")]
    [SerializeField] private float crawlSpeed = 1.5f;
    [SerializeField] private float crawlDuration = 1.0f;

    [Header("그로기 및 폭주 설정")]
    [SerializeField] private float enrageDuration = 10f;
    [SerializeField] private float groggyDuration = 5f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isJumping = false;
    private bool isCrawling = false;

    // 외부(EnemyHealth)에서 참조할 그로기 상태 프로퍼티
    public bool IsGroggy { get; private set; } = false;
    private bool isEnraged = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(SlimeMoveRoutine());
    }

    void Update()
    {
        if (animator != null && rb != null)
        {
            // 그로기 상태가 아닐 때만 이동 속도 파라미터 갱신
            float currentSpeed = IsGroggy ? 0f : rb.linearVelocity.magnitude;
            animator.SetFloat("MoveSpeed", currentSpeed);
            animator.SetBool("isJumping", isJumping);
            animator.SetBool("isGroggy", IsGroggy);
        }
    }

    private IEnumerator SlimeMoveRoutine()
    {
        while (true)
        {
            // 그로기(기절) 상태일 때는 아무것도 안 하고 루틴을 대기시킵니다.
            if (IsGroggy)
            {
                rb.linearVelocity = Vector2.zero;
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (player == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            float distance = Vector2.Distance(transform.position, player.position);

            // 폭주 상태일 때는 거리에 상관없이 무조건 플레이어에게 돌진 점프!
            if (isEnraged)
            {
                if (!isJumping && !isCrawling)
                {
                    yield return StartCoroutine(SlimeJump());
                    yield return new WaitForSeconds(defaultJumpInterval * 0.5f); // 폭주 시 점프 간격 단축
                }
            }
            else if (distance <= traceDistance)
            {
                if (!isJumping && !isCrawling)
                {
                    yield return StartCoroutine(SlimeJump());
                    yield return new WaitForSeconds(defaultJumpInterval);
                }
            }
            else
            {
                if (!isJumping && !isCrawling)
                {
                    yield return StartCoroutine(SlimeCrawl());
                    yield return new WaitForSeconds(defaultJumpInterval);
                }
            }
            yield return null;
        }
    }

    private IEnumerator SlimeCrawl()
    {
        if (IsGroggy) yield break;

        isCrawling = true;
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        HandleFlip(randomDir);

        float elapsed = 0f;
        while (elapsed < crawlDuration && !IsGroggy)
        {
            rb.linearVelocity = randomDir * crawlSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isCrawling = false;
    }

    private IEnumerator SlimeJump()
    {
        if (IsGroggy) yield break;

        isJumping = true;

        // 1. 트리거를 당겨 애니메이션 전환을 예약합니다.
        if (animator != null) animator.SetTrigger("isJumping");

        // -------------------------------------------------------------
        // ★ [핵심 해결책]: 유니티가 'Slime_Jump' 애니메이션으로 
        // 완전히 들어갈 때까지 코루틴을 잠시 대기시킵니다. (반 박자 딜레이 싱크 맞추기)
        // -------------------------------------------------------------
        if (animator != null)
        {
            // 다음 프레임으로 넘어가서 트리거가 반영되도록 1프레임 대기
            yield return null;

            // 현재 재생 중인 애니메이션 이름이 "Slime_Jump"가 될 때까지 무한 대기 (안전장치)
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Slime_Jump"))
            {
                yield return null;
            }
        }

        // 2. 이제 슬라임이 진짜 껑충 뛰어오르는 순간이므로, 여기서부터 방향과 힘을 계산합니다!
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        HandleFlip(direction);

        float elapsed = 0f;
        bool hasShaked = false;

        float actualJumpDuration = isEnraged ? 0.4f : jumpDuration;
        float actualJumpForce = isEnraged ? jumpForce * 1.5f : jumpForce;

        // 3. 진짜 도약한 순간부터 정확하게 체공 시간을 측정합니다.
        while (elapsed < actualJumpDuration)
        {
            if (IsGroggy)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.linearVelocity = direction * actualJumpForce;
            }

            // ❌ 기존 while 내부의 'if (elapsed >= actualJumpDuration * 0.95f)' 쉐이크 코드는 완전히 지워주세요!

            elapsed += Time.deltaTime;
            yield return null;
        }

        // -------------------------------------------------------------
        // ★ [위치 변경]: 점프 이동이 완전히 끝나고 바닥에 딱 멈추는 시점!
        // -------------------------------------------------------------
        rb.linearVelocity = Vector2.zero;

        // 💡 [착지 후 미세 딜레이 조절 레버]
        // 0f로 두면 바닥에 닿자마자 쿵! 흔들리고, 값을 높일수록(예: 0.05f, 0.1f) 착지 후 훨씬 더 늦게 흔들립니다.
        float landDelay = 0.08f;
        if (landDelay > 0f)
        {
            yield return new WaitForSeconds(landDelay);
        }

        // 완전히 멈추고(혹은 대기 후) 나서 확실하게 카메라를 흔들어줍니다.
        if (FollowingCamera.Instance != null)
        {
            FollowingCamera.Instance.Shake(0.25f, 0.2f, 8);
        }

        isJumping = false;
    }


    // [EnemyHealth.cs 에서 호출할 폭주 패턴 시작 메서드]
    public void TriggerEnragePattern()
    {
        if (!isEnraged && !IsGroggy)
        {
            StartCoroutine(EnrageAndGroggyRoutine());
        }
    }

    private IEnumerator EnrageAndGroggyRoutine()
    {
        // 1. 10초 폭주 모드 돌입
        isEnraged = true;
        Debug.Log("🔥 보스 슬라임 폭주 시작!");
        yield return new WaitForSeconds(enrageDuration);
        isEnraged = false;

        // 2. 폭주가 끝나면 5초 동안 그로기(기절) 모드 돌입
        IsGroggy = true;
        rb.linearVelocity = Vector2.zero; // 물리 이동 즉시 차단

        // 애니메이터 이름이 GroggyTrigger 혹은 GroggyTri 인지 에디터와 맞춰주세요.
        if (animator != null) animator.SetTrigger("GroggyTrigger");

        Debug.Log("💫 보스 슬라임 그로기(기절) 진입! 5초간 프리딜 타임");
        yield return new WaitForSeconds(groggyDuration);

        // 3. 기절 해제 후 정상 상태 복귀
        IsGroggy = false;
        Debug.Log("🛡️ 보스 슬라임 그로기 해제 및 정상 복귀");
    }

    private void HandleFlip(Vector2 dir)
    {
        if (spriteRenderer != null && !IsGroggy)
        {
            if (dir.x < 0) spriteRenderer.flipX = false;
            else if (dir.x > 0) spriteRenderer.flipX = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroggy) return; // 그로기 상태일 때는 공격 무시

        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                PlayerController playerCtrl = collision.gameObject.GetComponent<PlayerController>();
                if (playerCtrl != null) playerCtrl.OnHurt();
            }
        }
    }
}