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
            attackIndicator.transform.localRotation = Quaternion.identity;
            baseScale = attackIndicator.transform.localScale;

            SpriteRenderer indicatorSR = attackIndicator.GetComponent<SpriteRenderer>();
            if (indicatorSR != null)
            {
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

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        float originalAnimSpeed = 1f;
        if (animator != null)
        {
            originalAnimSpeed = animator.speed;
            animator.speed = 0f;
        }

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

        // -------------------------------------------------------------
        // ★ [수정]: 1초(noticeDuration) 동안 인디케이터가 차오르는 연출
        // -------------------------------------------------------------
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(true);
            attackIndicator.transform.localRotation = Quaternion.identity;

            float elapsed = 0f;
            float halfLen = baseLength / 2f;

            while (elapsed < noticeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / noticeDuration; // 0에서 1까지 증가하는 비율

                // 방향별로 중심축(Pivot) 정렬 상태에 맞춰 크기와 위치를 실시간 Lerp로 조절합니다.
                if (lookDirection == Vector2.right) // 오른쪽
                {
                    attackIndicator.transform.localScale = new Vector3(baseScale.x * progress, baseScale.y, 1f);
                    // 크기가 커질 때 앞으로 뻗어나가는 느낌을 주기 위해 중심 위치도 같이 이동
                    attackIndicator.transform.localPosition = new Vector3(halfLen * progress, -0.06f, 0f);
                }
                else if (lookDirection == Vector2.left) // 왼쪽
                {
                    attackIndicator.transform.localScale = new Vector3(baseScale.x * progress, baseScale.y, 1f);
                    attackIndicator.transform.localPosition = new Vector3(-halfLen * progress, -0.06f, 0f);
                }
                else if (lookDirection == Vector2.up) // 위
                {
                    attackIndicator.transform.localScale = new Vector3(baseScale.y, baseScale.x * progress, 1f);
                    attackIndicator.transform.localPosition = new Vector3(0f, (halfLen * progress) - 0.06f, 0f);
                }
                else if (lookDirection == Vector2.down) // 아래
                {
                    attackIndicator.transform.localScale = new Vector3(baseScale.y, baseScale.x * progress, 1f);
                    attackIndicator.transform.localPosition = new Vector3(0f, (-halfLen * progress) - 0.06f, 0f);
                }

                yield return null; // 매 프레임 대기하며 갱신
            }
        }

        // 5. 모든 칸이 다 채워지면 대쉬 돌진 실행
        if (attackIndicator != null) attackIndicator.SetActive(false);
        if (animator != null) animator.speed = originalAnimSpeed;

        rb.bodyType = RigidbodyType2D.Dynamic;

        float calculatedDuration = baseLength / dashSpeed;
        rb.linearVelocity = lookDirection * dashSpeed;

        yield return new WaitForSeconds(calculatedDuration);

        rb.linearVelocity = Vector2.zero;
        ResetRandomCooldown();
        isAttacking = false;
    }

    private void ResetRandomCooldown()
    {
        cooldownTimer = Random.Range(minCooldown, maxCooldown);
    }

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