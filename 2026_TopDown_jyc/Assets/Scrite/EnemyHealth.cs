using System.Collections; // 코루틴을 쓰기 위해 필수!
using UnityEngine;

public class EnemyHealth : Health
{
    [Header("몬스터 드롭 세팅")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

    [Header("넉백 세팅")]
    [SerializeField] private float knockbackForce = 1f; // 넉백 밀려나는 힘의 세기
    [SerializeField] private float knockbackDuration = 0.15f; // 넉백 유지 시간

    private Rigidbody2D rb;
    private bool isKnockback = false;

    protected override void Start()
    {
        // 부모(Health)의 Start 로직(SpriteRenderer 수집 등)을 먼저 실행합니다.
        base.Start();

        // 넉백 시 힘을 가하기 위해 내 몸의 Rigidbody 2D를 찾아둡니다.
        rb = GetComponent<Rigidbody2D>();
    }

    // ★ 부모의 TakeDamage를 이어받으면서 넉백 로직을 추가합니다!
    public override void TakeDamage(float damage)
    {
        // 부모의 원래 대미지 처리 로직(피 HP 깎기, 텍스트 띄우기, 연한빨강 깜빡임)을 그대로 실행
        base.TakeDamage(damage);

        // 아직 살아있고, 넉백 중이 아니라면 넉백 코루틴 실행!
        if (currentHealth > 0 && !isKnockback && rb != null)
        {
            // 플레이어의 위치를 찾아서 반대 방향으로 밀어냅니다.
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // 넉백 방향 계산: (몬스터 위치 - 플레이어 위치)를 하면 플레이어 반대 방향이 됩니다.
                Vector2 knockbackDirection = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;

                StartCoroutine(KnockbackRoutine(knockbackDirection));
            }
        }
    }

    // 넉백 동안 잠시 몬스터의 인공지능 이동을 멈추고 뒤로 밀어내는 코루틴
    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockback = true;

        // 플레이어 반대 방향으로 힘을 쾅! 가합니다.
        rb.linearVelocity = direction * knockbackForce;

        // 넉백 시간만큼 대기 (이 동안은 뒤로 밀려남)
        yield return new WaitForSeconds(knockbackDuration);

        // 넉백이 끝나면 속도를 다시 0으로 안전하게 초기화
        rb.linearVelocity = Vector2.zero;

        isKnockback = false;
    }

    public override void Die()
    {
        if (itemPrefab != null)
        {
            int dropCount = Random.Range(minDropCount, maxDropCount + 1);

            for (int i = 0; i < dropCount; i++)
            {
                Vector2 spawnOffset = new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
                Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;
                Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
            }
        }

        // 부모(Health)의 원래 Destroy 로직 실행
        base.Die();
    }
}