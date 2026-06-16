using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackDamage = 20f; // 플레이어의 공격력

    // 이 오브젝트(공격 범위)가 켜져 있는 동안 무언가 부딪히면 실행됩니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 대상의 태그가 "Enemy"인지 확인합니다.
        if (collision.CompareTag("Enemy"))
        {
            // 몬스터에게 붙어있는 Health(아까 우리가 만든 만능 체력 스크립트)를 가져옵니다.
            Health enemyHealth = collision.GetComponent<Health>();

            if (enemyHealth != null)
            {
                // 몬스터에게 대미지를 쾅! 입힙니다.
                enemyHealth.TakeDamage(attackDamage);
               // Debug.Log($"[플레이어 반격] 몬스터에게 {attackDamage}의 데미지를 입혔습니다!");
            }
        }
    }
}