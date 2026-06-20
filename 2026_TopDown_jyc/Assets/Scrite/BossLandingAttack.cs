using UnityEngine;

public class BossLandingAttack : MonoBehaviour
{
    [SerializeField] private float attackDamage = 15f; // 착지 충격파 대미지

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 평소엔 오브젝트가 꺼져있다가, 켜진 순간 플레이어가 범위 안에 있다면 대미지를 줍니다!
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);

                // 플레이어 피격 붉은 이펙트 연동
                PlayerController playerCtrl = collision.GetComponent<PlayerController>();
                if (playerCtrl != null) playerCtrl.OnHurt();

              //  Debug.Log("💥 보스 착지 충격파에 플레이어가 맞았습니다!");
            }
        }
    }
}