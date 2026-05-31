using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEditor.Progress;
public class PlayerController : MonoBehaviour
{
    

    public bool canMove = true; //대화

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

    // PlayerController.cs 내부

    // 에디터에서 전사, 마법사 등 모든 직업 데이터(JobData)를 배열로 등록해 둡니다.
    [Header("모든 직업 데이터 리스트")]
    public List<JobData> allJobs;

    void Start()
    {
        // 게임 시작 시 JSON에서 로드된 직업 이름을 가져옴
        string savedJobName = GameDataManager.Instance.playerData.currentJob;

        // 등록된 직업 리스트 중에서 일치하는 직업 데이터를 찾음
        JobData savedJob = allJobs.Find(job => job.jobName == savedJobName);

        // 찾았다면 해당 직업으로 세팅 (아까 만든 ChangeJob 함수 활용!)
        if (savedJob != null)
        {
            ChangeJob(savedJob);
        }
    }
    private void Update()
    {
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
                
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
        if (!canMove)//대화
            return;

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
        if (!canMove)
        {
            velocity = Vector2.zero;
            return;
        }//대화

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

            GameDataManager.Instance.playerData.coin += 1;

            Destroy(collision.gameObject);

            GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
        }

        if (collision.CompareTag("Respawn"))
        {

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (collision.CompareTag("Finish"))
        {
            collision.GetComponent<LevelObject>().MoveToNextLevel();
            
        }

    }
    public void ChangeJob(JobData newJob)
    {
        // 1. 외형(이미지) 변경
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && newJob.jobSprite != null)
        {
            spriteRenderer.sprite = newJob.jobSprite;
        }

        // 2. ★ 추가: 애니메이션 세트 변경
        Animator animator = GetComponent<Animator>();
        if (animator != null && newJob.jobAnimatorOverride != null)
        {
            // 플레이어의 애니메이터 컨트롤러를 새 직업의 애니메이션 세트로 교체!
            animator.runtimeAnimatorController = newJob.jobAnimatorOverride;
        }

        // 3. 스탯 변경
        this.moveSpeed = newJob.moveSpeed;

        Debug.Log($"{newJob.jobName} 애니메이션 및 스탯 적용 완료!");
    }

}
