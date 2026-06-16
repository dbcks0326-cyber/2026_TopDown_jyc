using System.Collections;
using UnityEngine;

public class EnemyAttackPattern : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private float noticeDuration = 1.0f; // 범위 예고 시간 (1초)
    [SerializeField] private float dashSpeed = 12f;        // 대쉬 속도
    [SerializeField] private float dashDamage = 20f;       // 대쉬 대미지
    [SerializeField] private float dashTriggerDistance = 5.0f; // 대쉬 발동 인식 거리

    [Header("대쉬 쿨타임 설정 (최소 ~ 최대)")]
    [SerializeField] private float minCooldown = 3.0f;
    [SerializeField] private float maxCooldown = 20.0f;

    [Header("공격 범위 자식 오브젝트 연결")]
    [SerializeField] private GameObject attackIndicator;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTransform;

    public bool isAttacking { get; private set; } = false;
    private float cooldownTimer = 0f;

    private Vector3 baseScale;
    private float baseLength;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (attackIndicator != null)
        {
            // 회전각을 0으로 고정하여 기준을 잡습니다.
            attackIndicator.transform.localRotation = Quaternion.identity;

            // 인스펙터에 이쁘게 맞춰두신 원본 스케일(X=0.25, Y=0.08)을 기억합니다.
            baseScale = attackIndicator.transform.localScale;

            SpriteRenderer indicatorSR = attackIndicator.GetComponent<SpriteRenderer>();
            if (indicatorSR != null)
            {
                // 우측 바라볼 때의 순수 월드 가로 길이를 정확히 측정합니다.
                baseLength = attackIndicator.transform.lossyScale.x * indicatorSR.sprite.bounds.size.x;
            }
            else
            {
                baseLength = 3f;
            }
        }
    }

    void Start()
    {
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) playerTransform = playerGO.transform;

        if (attackIndicator != null) attackIndicator.SetActive(false);

        ResetRandomCooldown();
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= dashTriggerDistance && cooldownTimer <= 0 && !isAttacking)
        {
            StartCoroutine(DashAttackRoutine());
        }
    }

    private IEnumerator DashAttackRoutine()
    {
        isAttacking = true;

        // 1. 차징 시 물리 및 애니메이션 완벽 정지 (얼음)
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        float originalAnimSpeed = 1f;
        if (animator != null)
        {
            originalAnimSpeed = animator.speed;
            animator.speed = 0f;
        }

        // 2. 방향 판단
        Vector2 lookDirection = Vector2.down;
        if (animator != null)
        {
            float dx = animator.GetFloat("DirX");
            float dy = animator.GetFloat("DirY");

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                lookDirection = dx > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                lookDirection = dy > 0 ? Vector2.up : Vector2.down;
            }
        }

        // 3. 범위 조절 및 위치 정렬 (회전 없이 크기와 부호 교체로 처리)
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(true);
            attackIndicator.transform.localRotation = Quaternion.identity;

            float halfLen = baseLength / 2f;

            if (lookDirection == Vector2.right) // 오른쪽
            {
                attackIndicator.transform.localScale = new Vector3(baseScale.x, baseScale.y, 1f);
                attackIndicator.transform.localPosition = new Vector3(halfLen, -0.06f, 0f);
            }
            else if (lookDirection == Vector2.left) // 왼쪽
            {
                attackIndicator.transform.localScale = new Vector3(baseScale.x, baseScale.y, 1f);
                attackIndicator.transform.localPosition = new Vector3(-halfLen, -0.06f, 0f);
            }
            else if (lookDirection == Vector2.up) // 위
            {
                attackIndicator.transform.localScale = new Vector3(baseScale.y, baseScale.x, 1f);
                attackIndicator.transform.localPosition = new Vector3(0f, halfLen - 0.06f, 0f);
            }
            else if (lookDirection == Vector2.down) // 아래
            {
                attackIndicator.transform.localScale = new Vector3(baseScale.y, baseScale.x, 1f);
                attackIndicator.transform.localPosition = new Vector3(0f, -halfLen - 0.06f, 0f);
            }
        }

        // 4. 1초 동안 가만히 대기
        yield return new WaitForSeconds(noticeDuration);

        // 5. 대쉬 돌진 실행
        if (attackIndicator != null) attackIndicator.SetActive(false);
        if (animator != null) animator.speed = originalAnimSpeed;

        rb.bodyType = RigidbodyType2D.Dynamic;

        // 기준 길이를 속도로 나누어 대쉬 거리를 정확히 일치시킴
        float calculatedDuration = baseLength / dashSpeed;
        rb.linearVelocity = lookDirection * dashSpeed;

        yield return new WaitForSeconds(calculatedDuration);

        // 6. 대쉬 종료 및 세팅 복구
        rb.linearVelocity = Vector2.zero;
        ResetRandomCooldown();
        isAttacking = false;
    }

    private void ResetRandomCooldown()
    {
        cooldownTimer = Random.Range(minCooldown, maxCooldown);
    }

    // ★ [수정]: 일반 물리 충돌(Is Trigger가 꺼져있을 때)에서도 대쉬 대미지가 들어가도록 추가
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAttacking && collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dashDamage);
                Debug.Log($"[대쉬 공격 성공(물리)] 플레이어 피 {dashDamage} 감소");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashTriggerDistance);
    }
}