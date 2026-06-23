using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;

    private float damage;

    private Vector2 direction;

    public void Init(Vector2 dir, float dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        transform.rotation =
    Quaternion.Euler(0, 0, angle + 90f);

        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)direction *
            speed *
            Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health hp =
            collision.GetComponent<Health>();

        if (hp != null &&
            collision.CompareTag("Enemy"))
        {
            hp.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    
}