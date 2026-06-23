using System.Collections;
using UnityEngine;

public class SlimeBossController : MonoBehaviour
{
    [Header("공격 설정")]
    // [삭제됨]: contactDamage 삭제 (이제 충격파 스크립트가 대미지를 줍니다)

    // ★ [추가]: 방금 만든 자식 오브젝트 LandingAttack을 연결할 칸
    [Header("착지 충격파 오브젝트 연결")]
    [SerializeField] private GameObject landingAttackObj;

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

    public bool IsGroggy { get; private set; } = false;
    private bool isEnraged = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // ★ [추가]: 게임 시작 시 착지 충격파 범위는 꺼둡니다.
        if (landingAttackObj != null) landingAttackObj.SetActive(false);

        StartCoroutine(SlimeMoveRoutine());
    }

    void Update()
    {
        if (animator != null && rb != null)
        {
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

            if (isEnraged)
            {
                if (!isJumping && !isCrawling)
                {
                    yield return StartCoroutine(SlimeJump());
                    yield return new WaitForSeconds(defaultJumpInterval * 0.5f);
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

        if (animator != null) animator.SetTrigger("isJumping");

        if (animator != null)
        {
            yield return null;
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Slime_Jump"))
            {
                yield return null;
            }
        }

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        HandleFlip(direction);

        float elapsed = 0f;
        float actualJumpDuration = isEnraged ? 0.4f : jumpDuration;
        float actualJumpForce = isEnraged ? jumpForce * 1.5f : jumpForce;

        // ───────────────────────────────────────────────────────────
        // ★ [추가]: 원래 크기 저장 및 목표 크기(1.4배) 설정
        // ───────────────────────────────────────────────────────────
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        // [1단계] 점프하며 공중에서 크기가 점점 1.4배로 커짐
        while (elapsed < actualJumpDuration)
        {
            if (IsGroggy)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.linearVelocity = direction * actualJumpForce;

                // 시간에 따라 크기를 부드럽게 키움 (0에서 1로 가면서 오리지널->1.4배)
                float t = elapsed / actualJumpDuration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 완전히 커진 상태 보장
        if (!IsGroggy) transform.localScale = targetScale;
        rb.linearVelocity = Vector2.zero;

        // [2단계] 착지 직전 순간(landDelay 동안) 원래 크기(1배)로 빠르게 압축
        float landDelay = 0.08f;
        if (landDelay > 0f)
        {
            float scaleElapsed = 0f;
            while (scaleElapsed < landDelay)
            {
                scaleElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(scaleElapsed / landDelay);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
        }

        // 원래 크기로 확실하게 고정
        transform.localScale = originalScale;

        // -------------------------------------------------------------
        // ★ [핵심 구현 지점]: 카메라가 흔들리는 쿵! 타이밍에 오브젝트 On/Off!
        // -------------------------------------------------------------
        if (FollowingCamera.Instance != null)
        {
            FollowingCamera.Instance.Shake(0.25f, 0.2f, 8);
        }

        Soundmanager.Instance.PlayBossImpact();
        // 플레이어 스킬처럼 착지 충격파를 순간적으로 켰다가 꺼줍니다!
        if (landingAttackObj != null && !IsGroggy)
        {
            landingAttackObj.SetActive(true);
            yield return new WaitForSeconds(0.15f); // 0.15초 동안 판정 유지
            landingAttackObj.SetActive(false);
        }

        isJumping = false;
    }

    public void TriggerEnragePattern()
    {
        if (!isEnraged && !IsGroggy)
        {
            StartCoroutine(EnrageAndGroggyRoutine());
        }
    }

    private IEnumerator EnrageAndGroggyRoutine()
    {
        isEnraged = true;
        Debug.Log("🔥 보스 슬라임 폭주 시작!");
        yield return new WaitForSeconds(enrageDuration);
        isEnraged = false;

        IsGroggy = true;
        rb.linearVelocity = Vector2.zero;

        // ★ [추가]: 그로기 걸리면 혹시 켜져있을지 모를 공격 판정은 확실하게 꺼줍니다.
        if (landingAttackObj != null) landingAttackObj.SetActive(false);

        if (animator != null) animator.SetTrigger("GroggyTrigger");

        Debug.Log("💫 보스 슬라임 그로기(기절) 진입! 5초간 프리딜 타임");
        yield return new WaitForSeconds(groggyDuration);

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

    // ★ [수정]: 기존 OnCollisionEnter2D 몸통 박치기 대미지 코드는 깔끔하게 삭제했습니다!
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 이제 닿는 것만으로는 대미지를 주지 않습니다. (충돌 밀려남 처리는 유지됨)
    }
}