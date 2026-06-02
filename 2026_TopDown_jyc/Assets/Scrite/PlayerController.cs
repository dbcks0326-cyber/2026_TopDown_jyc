using System.Collections; // ★ 코루틴(IEnumerator)을 쓰기 위해 반드시 필요합니다!
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // ★ 키보드 입력 체크(Keyboard.current)를 위해 필수입니다!
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public bool canMove = true; // 대화 상태 체크

    public float moveSpeed = 5f;

    [Header("기본 방향별 스프라이트 배열")]
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

    [Header("모든 직업 데이터 리스트")]
    public List<JobData> allJobs;

    // -------------------------------------------------------------
    // ★ 기존 추가: 피격 연출용 변수
    // -------------------------------------------------------------
    private bool isHurt = false; // 현재 빨간색 깜빡임 연출이 돌고 있는지 체크

    // -------------------------------------------------------------
    // ★ 기존 추가: 공격 시스템용 변수
    // -------------------------------------------------------------
    [Header("공격 설정")]
    public GameObject attackRangeObject; // 플레이어 자식으로 만든 AttackRange 오브젝트 등록 칸
    public float attackCooldown = 0.3f;  // 공격 연사 속도 (쿨타임)
    private float lastAttackTime = 0f;    // 마지막으로 공격한 시간 저장용
    private bool isAttacking = false;    // 현재 공격 애니메이션/루틴이 돌고 있는지 체크

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    void Start()
    {
        // 1. 게임 시작 시 JSON에서 로드된 직업 이름을 가져옴
        string savedJobName = GameDataManager.Instance.playerData.currentJob;

        // 등록된 직업 리스트 중에서 일치하는 직업 데이터를 찾음
        JobData savedJob = allJobs.Find(job => job.jobName == savedJobName);

        // 찾았다면 해당 직업으로 세팅
        if (savedJob != null)
        {
            ChangeJob(savedJob);
        }

        // -------------------------------------------------------------
        // ★ 기존 코드: 특정 스테이지 이동 시 풀피가 된 세이브 데이터를 내 몸(Health)에 동기화
        // -------------------------------------------------------------
        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.Invoke("Start", 0.01f);
            Debug.Log("[PlayerController] 씬 전환에 따른 플레이어 체력 스탯 동기화 완료.");
        }

 
    }

    private void Update()
    {
        // -------------------------------------------------------------
        // ★ 코드 내 직접 키 지정: 매 프레임마다 키보드 E 키가 눌렸는지 감지합니다.
        // -------------------------------------------------------------
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // 이동 가능한 상태이고, 현재 이미 공격 중이 아니라면 공격 조건 확인
            if (canMove && !isAttacking)
            {
                // 공격 쿨타임이 지났는지 검사
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    StartCoroutine(AttackRoutine());
                }
            }
        }

        // 공격 중일 때는 걷는 스프라이트 애니메이션이 도는 것을 잠시 멈춥니다.
        if (isAttacking) return;

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
        // 대화 중이거나, 공격 동작을 수행 중일 때는 움직이지 못하게 잠금 처리
        if (!canMove || isAttacking)
        {
            rb.linearVelocity = Vector2.zero; // 물리 속도 초기화로 미끄러짐 방지
            return;
        }

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
        // ★ 버그 해결: 키를 누르거나 '떼는' 입력값은 상태와 상관없이 무조건 실시간으로 최신화합니다.
        // 이렇게 해야 공격 중에 키에서 손을 떼도 input이 (0, 0)이 되어 미끄러지지 않습니다.
        input = Value.Get<Vector2>();

        // 대화 중이거나 공격 중일 때는 실제 물리 속도(velocity)로 연결되는 것을 차단합니다.
        if (!canMove || isAttacking)
        {
            velocity = Vector2.zero;
            return;
        }

        // 정상 상태일 때만 실제 이동 속도를 계산합니다.
        velocity = input.normalized * moveSpeed;

        if (input.sqrMagnitude > 0.01f)
        {
            // -------------------------------------------------------------
            // ★ 플레이어님이 직접 커스텀하신 위치 및 각도 값 그대로 유지
            // -------------------------------------------------------------
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                {
                    ChangeSprites(spriteRight);
                    // [오른쪽]
                    if (attackRangeObject != null)
                    {
                        attackRangeObject.transform.localPosition = new Vector2(0.1f, 0f);
                        attackRangeObject.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    }
                }
                else
                {
                    ChangeSprites(spriteLeft);
                    // [왼쪽]
                    if (attackRangeObject != null)
                    {
                        attackRangeObject.transform.localPosition = new Vector2(-0.1f, 0f);
                        attackRangeObject.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                    }
                }
            }
            else
            {
                if (input.y > 0)
                {
                    ChangeSprites(spriteUp);
                    // [위쪽]
                    if (attackRangeObject != null)
                    {
                        attackRangeObject.transform.localPosition = new Vector2(0f, 0.1f);
                        attackRangeObject.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
                    }
                }
                else
                {
                    ChangeSprites(spriteDown);
                    // [아래쪽]
                    if (attackRangeObject != null)
                    {
                        attackRangeObject.transform.localPosition = new Vector2(0f, -0.1f);
                        attackRangeObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    }
                }
            }
            // -------------------------------------------------------------
        }
    }

    // -------------------------------------------------------------
    // ★ 신규 추가: 공격 범위를 순간적으로 켰다 끄는 코루틴 함수
    // -------------------------------------------------------------
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 공격 순간에는 기존 물리 속도를 zero로 만들어 즉시 정지 (미끄러짐 방지)
        rb.linearVelocity = Vector2.zero;

        if (attackRangeObject != null)
        {
            // 1. 공격용 콜라이더 오브젝트 활성화 (부딪힌 적에게 데미지)
            attackRangeObject.SetActive(true);
        }

        // 2. 무기를 휘두르는 판정 시간 (0.15초 동안 유지)
        yield return new WaitForSeconds(0.15f);

        if (attackRangeObject != null)
        {
            // 3. 판정이 끝났으므로 다시 오브젝트 비활성화
            attackRangeObject.SetActive(false);
        }

        isAttacking = false;

        // -------------------------------------------------------------
        // ★ 추가: 공격이 끝난 직후, 만약 플레이어가 ASDW 키를 여전히 누르고 있다면
        // 그 입력값(input)을 바탕으로 속도(velocity)를 즉시 부활시켜 줍니다!
        // -------------------------------------------------------------
        if (input.sqrMagnitude > 0.01f)
        {
            velocity = input.normalized * moveSpeed;
        }
    }

    // -------------------------------------------------------------
    // ★ 기존 추가: 외부(Health.cs)에서 호출할 피격 시작 함수
    // -------------------------------------------------------------
    public void OnHurt()
    {
        if (isHurt) return;
        StartCoroutine(HurtRoutine());
    }

    // -------------------------------------------------------------
    // ★ 기존 추가: 실시간으로 색상을 바꿨다 되돌리는 코루틴 함수
    // -------------------------------------------------------------
    private IEnumerator HurtRoutine()
    {
        isHurt = true;

        if (sr != null)
        {
            // 플레이어도 부드러운 연빨강으로 변경! (0.15초 유지로 타이밍도 통일)
            sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        }

        yield return new WaitForSeconds(0.15f);

        if (sr != null)
        {
            sr.color = Color.white;
        }

        isHurt = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 코인(아이템) 충돌 처리
        if (collision.CompareTag("Coin"))
        {
            itemOB coinItem = collision.GetComponent<itemOB>();

            if (coinItem != null)
            {
                GameDataManager.Instance.playerData.collectedItems.Add(coinItem.GetItemName());
                GameDataManager.Instance.playerData.coin += coinItem.GetCoin();

                Debug.Log($"[플레이어 주체 획득] {coinItem.GetItemName()} 획득! (+{coinItem.GetCoin()}코인) / 총 코인: {GameDataManager.Instance.playerData.coin}개");

                Destroy(collision.gameObject);
            }
        }

        // 2. 낙사 구역 및 리스폰 처리
        if (collision.CompareTag("Respawn"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        // 3. 스테이지 클리어 및 다음 레벨 이동 처리
        if (collision.CompareTag("Finish"))
        {
            GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
    }

    public void ChangeJob(JobData newJob)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && newJob.jobSprite != null)
        {
            spriteRenderer.sprite = newJob.jobSprite;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null && newJob.jobAnimatorOverride != null)
        {
            animator.runtimeAnimatorController = newJob.jobAnimatorOverride;
        }

        this.moveSpeed = newJob.moveSpeed;

        Debug.Log($"{newJob.jobName} 애니메이션 및 스탯 적용 완료!");
    }
}