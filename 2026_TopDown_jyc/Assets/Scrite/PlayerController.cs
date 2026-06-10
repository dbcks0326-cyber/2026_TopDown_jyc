using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal; // ★ 중요: Light2D 컴포넌트를 쓰기 위해 반드시 필요합니다!

public class PlayerController : MonoBehaviour
{
    public bool canMove = true;

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

    // 피격 연출용 변수
    private bool isHurt = false;

    // -------------------------------------------------------------
    // ★ Q, E, R 독립 스킬 설정 (오브젝트, 대미지, 라이트 데이터 연동)
    // -------------------------------------------------------------
    [Header("Q 스킬 설정")]
    [SerializeField] private GameObject qAttackPrefab;
    [SerializeField] private float qDamage = 15f;
    [SerializeField] private float qCooldown = 3f;
    private float nextQAttackTime = 0f;

    [Header("E 스킬 설정 (기본공격)")]
    [SerializeField] private GameObject eAttackPrefab;
    [SerializeField] private float eDamage = 10f;
    [SerializeField] private float eCooldown = 0.3f;
    private float nextEAttackTime = 0f;

    [Header("R 스킬 설정 (궁극기)")]
    [SerializeField] private GameObject rAttackPrefab;
    [SerializeField] private float rDamage = 40f;
    [SerializeField] private float rCooldown = 10f;
    private float nextRAttackTime = 0f;

    private bool isAttacking = false;

    [Header("쿨타임 UI 이미지 연결")]
    [SerializeField] private Image qCooldownImage;
    [SerializeField] private Image eCooldownImage;
    [SerializeField] private Image rCooldownImage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    void Start()
    {
        string savedJobName = GameDataManager.Instance.playerData.currentJob;
        JobData savedJob = allJobs.Find(job => job.jobName == savedJobName);
        if (savedJob != null)
        {
            ChangeJob(savedJob);
        }

        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.Invoke("Start", 0.01f);
        }

        if (qCooldownImage != null) qCooldownImage.fillAmount = 0;
        if (eCooldownImage != null) eCooldownImage.fillAmount = 0;
        if (rCooldownImage != null) rCooldownImage.fillAmount = 0;

        // 시작할 때 모든 스킬 오브젝트는 꺼둡니다.
        if (qAttackPrefab != null) qAttackPrefab.SetActive(false);
        if (eAttackPrefab != null) eAttackPrefab.SetActive(false);
        if (rAttackPrefab != null) rAttackPrefab.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (canMove && !isAttacking)
            {
                float moveX = 0f;
                float moveY = 0f;

                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                if (Keyboard.current.dKey.isPressed) moveX = 1f;
                if (Keyboard.current.sKey.isPressed) moveY = -1f;
                if (Keyboard.current.wKey.isPressed) moveY = 1f;

                input = new Vector2(moveX, moveY);
                velocity = input.normalized * moveSpeed;

                HandleDirectionRotation();
            }

            // Q 키 입력 체크
            if (Keyboard.current.qKey.wasPressedThisFrame && canMove && !isAttacking)
            {
                if (Time.time >= nextQAttackTime)
                {
                    nextQAttackTime = Time.time + qCooldown;
                    StartCoroutine(AttackRoutine(qAttackPrefab, qDamage));
                }
            }

            // E 키 입력 체크
            if (Keyboard.current.eKey.wasPressedThisFrame && canMove && !isAttacking)
            {
                if (Time.time >= nextEAttackTime)
                {
                    nextEAttackTime = Time.time + eCooldown;
                    StartCoroutine(AttackRoutine(eAttackPrefab, eDamage));
                }
            }

            // R 키 입력 체크
            if (Keyboard.current.rKey.wasPressedThisFrame && canMove && !isAttacking)
            {
                if (Time.time >= nextRAttackTime)
                {
                    nextRAttackTime = Time.time + rCooldown;
                    StartCoroutine(AttackRoutine(rAttackPrefab, rDamage));
                }
            }
        }

        UpdateCooldownUI(nextQAttackTime, qCooldown, qCooldownImage);
        UpdateCooldownUI(nextEAttackTime, eCooldown, eCooldownImage);
        UpdateCooldownUI(nextRAttackTime, rCooldown, rCooldownImage);

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
            if (frameIndex >= currentSprites.Length) frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        if (!canMove || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void HandleDirectionRotation()
    {
        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                {
                    ChangeSprites(spriteRight);
                    SetAllAttackTransforms(new Vector2(0.14f, -0.01f), new Vector3(0f, 0f, 90f));
                }
                else
                {
                    ChangeSprites(spriteLeft);
                    SetAllAttackTransforms(new Vector2(-0.14f, -0.01f), new Vector3(0f, 0f, 270f));
                }
            }
            else
            {
                if (input.y > 0)
                {
                    ChangeSprites(spriteUp);
                    SetAllAttackTransforms(new Vector2(0f, 0.14f), new Vector3(0f, 0f, 180f));
                }
                else
                {
                    ChangeSprites(spriteDown);
                    SetAllAttackTransforms(new Vector2(0f, -0.14f), new Vector3(0f, 0f, 0f));
                }
            }
        }
    }

    private void SetAllAttackTransforms(Vector2 pos, Vector3 rot)
    {
        SetSingleTransform(qAttackPrefab, pos, rot);
        SetSingleTransform(eAttackPrefab, pos, rot);
        SetSingleTransform(rAttackPrefab, pos, rot);
    }

    private void SetSingleTransform(GameObject obj, Vector2 pos, Vector3 rot)
    {
        if (obj != null)
        {
            obj.transform.localPosition = pos;
            obj.transform.localEulerAngles = rot;
        }
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites) return;
        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        sr.sprite = currentSprites[frameIndex];
    }

    public void OnMove(InputValue Value) { }

    // -------------------------------------------------------------
    // ★ [수정] 스킬 애니메이션 시간에 비례해 불빛 밝기(intensity)를 0 -> 2 -> 0 제어
    // -------------------------------------------------------------
    private IEnumerator AttackRoutine(GameObject attackPrefab, float skillDamage)
    {
        if (attackPrefab == null) yield break;

        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        velocity = Vector2.zero;

        // 1. 스킬 오브젝트 켜기
        attackPrefab.SetActive(true);

        // [참고] 공격 대미지 연동 부분
        PlayerAttack attackScript = attackPrefab.GetComponent<PlayerAttack>();
        if (attackScript != null)
        {
            // attackScript.damage = skillDamage; 
        }

        // 2. 애니메이션 실제 시간 계산
        Animator effectAnim = attackPrefab.GetComponent<Animator>();
        float attackDuration = 0.15f; // 기본 예외처리 대기 시간

        if (effectAnim != null)
        {
            attackDuration = effectAnim.GetCurrentAnimatorStateInfo(0).length;
        }

        // 3. 해당 스킬 오브젝트 혹은 그 자식에게 붙어있는 Light2D 컴포넌트 탐색
        Light2D skillLight = attackPrefab.GetComponentInChildren<Light2D>();

        float elapsedTime = 0f;

        // 애니메이션 시간이 흐르는 동안 실시간 루프 연산 실행
        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;

            // 현재 진행률을 0과 1 사이 비율(t)로 계산
            float t = elapsedTime / attackDuration;

            if (skillLight != null)
            {
                // 수학 함수인 Mathf.Sin을 이용하여 불빛이 0에서 스르륵 2까지 커졌다가 다시 0으로 부드럽게 줄어들게 만듭니다.
                // Sin(0) = 0, Sin(π/2) = 1, Sin(π) = 0 임을 이용한 정석 공식입니다.
                skillLight.intensity = Mathf.Sin(t * Mathf.PI) * 2f;
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 4. 안전장치: 시간이 다 끝나면 확실하게 불빛을 끄고 오브젝트 비활성화
        if (skillLight != null)
        {
            skillLight.intensity = 0f;
        }

        attackPrefab.SetActive(false);
        isAttacking = false;
    }

    public void OnHurt()
    {
        if (isHurt) return;
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        isHurt = true;
        if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.15f);
        if (sr != null) sr.color = Color.white;
        isHurt = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            
            itemOB coinItem = collision.GetComponent<itemOB>();
            if (coinItem != null)
            {
                GameDataManager.Instance.playerData.collectedItems.Add(coinItem.GetItemName());
                GameDataManager.Instance.playerData.coin += coinItem.GetCoin();
                Destroy(collision.gameObject);
            }
        }
        if (collision.CompareTag("Respawn"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        if (collision.CompareTag("Finish"))
        {
            GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
    }

    public void ChangeJob(JobData newJob)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && newJob.jobSprite != null) spriteRenderer.sprite = newJob.jobSprite;
        Animator animator = GetComponent<Animator>();
        if (animator != null && newJob.jobAnimatorOverride != null) animator.runtimeAnimatorController = newJob.jobAnimatorOverride;
        this.moveSpeed = newJob.moveSpeed;
    }

    private void UpdateCooldownUI(float nextUseTime, float cooldownDuration, Image cooldownImage)
    {
        if (cooldownImage == null) return;
        float timeLeft = nextUseTime - Time.time;
        if (timeLeft > 0) cooldownImage.fillAmount = timeLeft / cooldownDuration;
        else cooldownImage.fillAmount = 0f;
    }
}