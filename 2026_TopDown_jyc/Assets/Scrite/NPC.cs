using TMPro;
using UnityEngine;

using System.Collections;

public class SignPost : MonoBehaviour
{
    [Header("설정")]
    public string message = "이곳은 평화로운 마을입니다.";
    public TextMeshProUGUI uiText; // UI 텍스트 연결
    public float fadeSpeed = 2f;

    private Coroutine fadeCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(message);
        // 플레이어가 근처에 오면
        if (collision.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            uiText.text = message;
            fadeCoroutine = StartCoroutine(FadeText(1f)); // 나타나기
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 플레이어가 멀어지면
        if (collision.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeText(0f)); // 사라지기
        }
    }

    IEnumerator FadeText(float targetAlpha)
    {
        float startAlpha = uiText.color.a;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeSpeed;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer);
            uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, newAlpha);
            yield return null;
        }
    }
}