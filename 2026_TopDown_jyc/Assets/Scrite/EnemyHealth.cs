using System.Collections;
using UnityEngine;

public class EnemyHealth : Health
{
    [Header("보스 설정")]
    [SerializeField] private bool isBoss = false;
    [SerializeField] private bool canKnockback = false;
    private SlimeBossController bossController;

    [Header("데미지 텍스트")]
    [SerializeField] private GameObject damageTextPrefab;

    [Header("몬스터 드롭 세팅")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

    [Header("넉백 세팅")]
    [SerializeField] private float knockbackForce = 1f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("스폰 무적 설정")]
    [SerializeField] private float spawnInvincibleDuration = 1.0f;

    private bool isSpawnInvincible = false;

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

        StartCoroutine(SpawnInvincibleRoutine());
    }

    private IEnumerator SpawnInvincibleRoutine()
    {
        isSpawnInvincible = true;

        yield return new WaitForSeconds(spawnInvincibleDuration);

        isSpawnInvincible = false;
    }

    public override void TakeDamage(float damage)
    {
        if (isSpawnInvincible)
        {
            Debug.Log($"{gameObject.name}: 스폰 무적 상태라 대미지를 받지 않습니다.");
            return;
        }

        if (isBoss && bossController != null && bossController.IsGroggy)
        {
            damage *= 1.5f;
        }

        // 데미지 텍스트 생성
        ShowDamageText(damage);

        base.TakeDamage(damage);

        if (isBoss)
        {
            UpdateBossHealthUI();
        }

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

        if (currentHealth > 0 && !isKnockback && rb != null)
        {
            if (isBoss && !canKnockback) return;

            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                Vector2 knockbackDirection =
                    ((Vector2)transform.position - (Vector2)player.transform.position).normalized;

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
                Vector2 spawnOffset = new Vector2(
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f)
                );

                Vector2 spawnPosition =
                    (Vector2)transform.position + spawnOffset;

                Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
            }
        }

        if (isBoss && BossHPBar.Instance != null)
        {
            BossHPBar.Instance.HideHPBar();
        }

        base.Die();
    }
    private void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null)
            return;

        Vector3 spawnPos = transform.position +
    new Vector3(
        Random.Range(-0.2f, 0.2f),
        Random.Range(-0.2f, 0.2f),
        0f
    );

        GameObject damageObj =
            Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        DamageText damageText =
            damageObj.GetComponent<DamageText>();

        if (damageText != null)
        {
            damageText.SetDamage(damage);
        }
    }
}