using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackDamage = 20f; // 기본 대미지 (예비용)

    // ★ [추가]: 외부(PlayerController)에서 스킬별 대미지를 주입해 주는 메서드
    public void SetAttackDamage(float damage)
    {
        attackDamage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Health enemyHealth = collision.GetComponent<Health>();

            if (enemyHealth != null)
            {
                // 이제 주입받은 대미지로 몬스터를 때립니다!
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }
}