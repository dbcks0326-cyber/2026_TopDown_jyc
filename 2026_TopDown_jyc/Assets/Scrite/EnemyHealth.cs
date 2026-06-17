using System.Collections;
using UnityEngine;

public class EnemyHealth : Health
{
    [Header("보스 설정")]
    [SerializeField] private bool isBoss = false;             // 이 몬스터가 보스인지 여부
    [SerializeField] private bool canKnockback = false;        // 보스가 넉백을 당할지 여부 (보통 false)
    private SlimeBossController bossController;

    [Header("몬스터 드롭 세팅")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

    [Header("넉백 세팅")]
    [SerializeField] private float knockbackForce = 1f;      // 일반 몬스터 넉백 힘
    [SerializeField] private float knockbackDuration = 0.15f; // 넉백 지속 시간

    private Rigidbody2D rb;
    private bool isKnockback = false;
    private bool enrageUsed = false;                          // 폭주 패턴 중복 실행 방지 스위치

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();

        // 만약 보스라면 보스 컨트롤러 컴포넌트를 미리 가져옵니다.
        if (isBoss)
        {
            bossController = GetComponent<SlimeBossController>();
        }
    }

    public override void TakeDamage(float damage)
    {
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

        // 4. [보스 특권]: 체력이 30% 이하로 떨어지면 딱 한 번 폭주 패턴 가동! (실수형 캐스팅 버그 방지)
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
            // 보스인데 넉백 면역(canKnockback = false) 상태라면 넉백 코루틴을 씹어버립니다.
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

    // ★ 보스 HP 바 UI를 업데이트하는 메서드 (기존 구현 방식이 있다면 그에 맞게 연동하세요)
    private void UpdateBossHealthUI()
    {
        // 예시: BossUIManager.Instance.UpdateHP(currentHealth, maxHP);
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

        base.Die();
    }
}