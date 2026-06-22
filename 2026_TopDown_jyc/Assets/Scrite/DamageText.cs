using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float lifeTime = 0.4f;

    private TextMeshPro textMesh;
    private Vector3 velocity;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void SetDamage(float damage)
    {
        textMesh.text = damage.ToString("0");
    }

    private void Start()
    {
        velocity = new Vector3(
            Random.Range(-0.2f, 0.2f),
            0.5f,
            0f
        );

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += Vector3.up * 0.05f * Time.deltaTime;
    }
}