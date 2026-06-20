using System.Collections;
using UnityEngine;

public class EnemyHealth : Health
{
    [Header("보스 설정")]
    [SerializeField] private bool isBoss = false;
    [SerializeField] private bool canKnockback = false;
    private SlimeBossController bossController;

    [Header("몬스터 드롭 세팅")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

    [Header("넉백 세팅")]
    [SerializeField] private float knockbackForce = 1f;
    [SerializeField] private float knockbackDuration = 0.15f;

    // ───────────────────────────────────────────────────────────
    // ★ [추가]: 스폰 무적 관련 변수
    // ───────────────────────────────────────────────────────────
    [Header("스폰 무적 설정")]
    [SerializeField] private float spawnInvincibleDuration = 1.0f; // 무적 시간 (1초)
    private bool isSpawnInvincible = false;                        // 현재 무적 상태인가?

    private Rigidbody2D rb;
    private bool isKnockback = false;
    private bool enrageUsed = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();

        if (isBoss)
        {
            bossController = GetComponent<SlimeBossController>();

            if (BossHPBar.Instance != null)
            {
                BossHPBar.Instance.ShowHPBar();
            }
        }

        // ───────────────────────────────────────────────────────────
        // ★ [추가]: 태어나자마자 1초 무적 코루틴 가동!
        // ───────────────────────────────────────────────────────────
        StartCoroutine(SpawnInvincibleRoutine());
    }

    // ───────────────────────────────────────────────────────────
    // ★ [추가]: 스폰 무적 타임 제어 코루틴
    // ───────────────────────────────────────────────────────────
    private IEnumerator SpawnInvincibleRoutine()
    {
        isSpawnInvincible = true;

        // 투명도를 조절해서 무적 상태임을 연출하고 싶다면 반투명하게 바꿀 수 있습니다.
        // 예: GetComponentInChildren<SpriteRenderer>().color = new Color(1,1,1, 0.5f);

        yield return new WaitForSeconds(spawnInvincibleDuration);

        isSpawnInvincible = false;

        // 무적이 끝나면 원래 색상으로 복구
        // 예: GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }

    public override void TakeDamage(float damage)
    {
        // ───────────────────────────────────────────────────────────
        // ★ [추가]: 현재 스폰 무적 상태라면 아래 대미지 계산을 전부 씹어버림!
        // ───────────────────────────────────────────────────────────
        if (isSpawnInvincible)
        {
            Debug.Log($"{gameObject.name}: 스폰 무적 상태라 대미지를 받지 않습니다.");
            return;
        }

        // 1. [보스 특권]: 만약 보스가 현재 그로기(기절) 상태라면 대미지를 1.5배로 증폭해서 받습니다.
        if (isBoss && bossController != null && bossController.IsGroggy)
        {
            damage *= 1.5f;
        }

        // 2. 부모의 대미지 처리 (currentHealth 감소)
        base.TakeDamage(damage);

        // 3. [보스 특권]: 체력이 깎였으니 실시간으로 보스 HP 바 UI를 갱신합니다.
        if (isBoss)
        {
            UpdateBossHealthUI();
        }

        // 4. [보스 특권]: 체력이 30% 이하로 떨어지면 딱 한 번 폭주 패턴 가동!
        if (isBoss && bossController != null && !enrageUsed)
        {
            float healthRatio = (float)currentHealth / maxHP;
            if (healthRatio <= 0.3f && currentHealth > 0)
            {
                enrageUsed = true;
                bossController.TriggerEnragePattern();
                Debug.Log($"🔥 보스 체력 30% 이하 진입 ({healthRatio * 100}%). 폭주 및 그로기 시스템 가동!");
            }
        }

        // 5. 넉백 처리 (살아있고 넉백 중이 아닐 때)
        if (currentHealth > 0 && !isKnockback && rb != null)
        {
            if (isBoss && !canKnockback) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector2 knockbackDirection = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
                StartCoroutine(KnockbackRoutine(knockbackDirection));
            }
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockback = true;
        rb.linearVelocity = direction * knockbackForce;
        yield return new WaitForSeconds(knockbackDuration);
        rb.linearVelocity = Vector2.zero;
        isKnockback = false;
    }

    private void UpdateBossHealthUI()
    {
        if (BossHPBar.Instance != null)
        {
            BossHPBar.Instance.UpdateHP(currentHealth, maxHP);
        }
    }

    public override void Die()
    {
        if (itemPrefab != null)
        {
            int dropCount = Random.Range(minDropCount, maxDropCount + 1);
            for (int i = 0; i < dropCount; i++)
            {
                Vector2 spawnOffset = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;
                Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
            }
        }

        if (isBoss && BossHPBar.Instance != null)
        {
            BossHPBar.Instance.HideHPBar();
        }

        base.Die();
    }
}