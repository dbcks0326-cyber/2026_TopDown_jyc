using System.Collections;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDead = false;

    [Header("사망 연출 옵션")]
    [SerializeField] private float fadeSpeed = 2f;
    
   

    [Tooltip("숫자가 클수록 180도로 빠르게 뒤집힙니다.")]
    [SerializeField] private float rotationSpeed = 10f;

    void Awake()
    {
        TryGetComponent(out spriteRenderer);
        TryGetComponent(out rb);
        TryGetComponent(out col);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (col != null) col.enabled = false;

        StartCoroutine(DeathAnimationRoutine());
    }

    IEnumerator DeathAnimationRoutine()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            float randomX = Random.Range(-0.2f, 0.2f);
            rb.linearVelocity = new Vector2(randomX, -0.1f);
        }

        // 최종적으로 도달할 목표 각도 (180도 뒤집힌 상태)
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 180f);

        float timer = 0f;

        // 크기 조절 코드를 빼고, 투명도가 남아있거나 지정한 시간 동안만 루프를 돕니다.
        // 기존 조건문 대신 투명도(Alpha)가 0보다 클 때만 돌도록 수정
        while (spriteRenderer != null && spriteRenderer.color.a > 0)
        {
            timer += Time.deltaTime;

            // 1) 투명도 줄이기
            Color currentColor = spriteRenderer.color;
            currentColor.a -= Time.deltaTime * fadeSpeed;
            spriteRenderer.color = currentColor;

            // 2) 부드럽게 돌아가는 애니메이션
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            yield return null;
        }

        // 이제 투명도가 0이 되자마자 바로 하이어라키에서 삭제됩니다!
        Destroy(gameObject);
    }
}