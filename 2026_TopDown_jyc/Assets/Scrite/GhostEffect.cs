using UnityEngine;

public class GhostEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    [SerializeField] private float fadeDuration = 0.4f; // 사라지는 시간
    private float timer = 0f;
    private Color startColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);
        sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}