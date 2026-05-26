using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEditor.Progress;
public class PlayerController : MonoBehaviour
{
    float Coin;

    public float moveSpeed = 5f;

    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;

    public float frameTim = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 input;
    private Vector2 velocity;

    private Sprite[] currentSprites;

    private int frameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    private void Update()
    {
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
            return;
        }

        timer += Time.deltaTime;

        if (timer >= frameTim)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= currentSprites.Length)
                frameIndex = 0;

            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites)
            return;

        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        sr.sprite = currentSprites[frameIndex];
    }

    public void OnMove(InputValue Value)
    {
        input = Value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                    ChangeSprites(spriteRight);
                else
                    ChangeSprites(spriteLeft);
            }
            else
            {
                if (input.y > 0)
                    ChangeSprites(spriteUp);
                else
                    ChangeSprites(spriteDown);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
           itemOB Coin = collision.GetComponent<itemOB>();

            GameDataManager.Instance.playerData.collectedItems.Add(Coin.GetItemName());

            Destroy(collision.gameObject);

            GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);

        }

        if (collision.CompareTag("Respawn"))
        {
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
    }
}
