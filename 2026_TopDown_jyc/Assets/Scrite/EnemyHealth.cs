using UnityEngine;

public class EnemyHealth : Health
{
    [Header("몬스터 드롭 세팅")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int minDropCount = 1;
    [SerializeField] private int maxDropCount = 3;

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