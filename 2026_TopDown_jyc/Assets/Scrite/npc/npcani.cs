using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePlayer : MonoBehaviour
{
    public float fps = 0.05f;
    public Sprite[] frames;

    private SpriteRenderer sr;
    private Coroutine animCoroutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        // 코루틴 핸들을 저장
        animCoroutine = StartCoroutine(StartAnimation());
    }

    IEnumerator StartAnimation()
    {
        while (true)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                sr.sprite = frames[i];
                yield return new WaitForSeconds(fps);
            }
        }
    }
}