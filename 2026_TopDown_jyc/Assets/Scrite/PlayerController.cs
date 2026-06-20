using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public bool canMove = true;
    public float moveSpeed = 5f;

    [Header("대쉬 잔상 설정")]
    [SerializeField] private GameObject ghostPrefab; // Inspector에서 잔상용 프리팹을 연결하세요!
    [SerializeField] private float ghostInterval = 0.05f;

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
    private Vector2 lastMoveDirection = Vector2.down; // ★ 마지막에 바라본 방향 백업용 (대쉬 방향 기준점)

    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    [Header("모든 직업 데이터 리스트")]
    public List<JobData> allJobs;

    private bool isHurt = false;

    // -------------------------------------------------------------
    // ★ Q 대쉬 스킬 신규 설정 변경
    // -------------------------------------------------------------
    [Header("Q 스킬 설정 (대쉬)")]
    [SerializeField] private float dashSpeed = 15f;        // 대쉬 속도
    [SerializeField] private float dashDuration = 0.2f;     // 대쉬 지속 시간 (몇 초 동안 돌진할지)
    [SerializeField] private float qCooldown = 3f;          // 대쉬 쿨타임
    private float nextQAttackTime = 0f;
    private bool isDashing = false;                         // 현재 대쉬 중인지 체크

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

        if (eAttackPrefab != null) eAttackPrefab.SetActive(false);
        if (rAttackPrefab != null) rAttackPrefab.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            // -------------------------------------------------------------
            // ★ [수정]: 조건문에서 !isAttacking을 제거했습니다.
            // 이제 공격 스킬(E, R)이 켜져 있는 중에도 키보드 이동 입력을 정상적으로 받습니다!
            // -------------------------------------------------------------
            if (canMove && !isDashing)
            {
                float moveX = 0f;
                float moveY = 0f;

                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                if (Keyboard.current.dKey.isPressed) moveX = 1f;
                if (Keyboard.current.sKey.isPressed) moveY = -1f;
                if (Keyboard.current.wKey.isPressed) moveY = 1f;

                input = new Vector2(moveX, moveY);
                velocity = input.normalized * moveSpeed;

                // 마지막으로 바라본 유효한 방향을 항상 기억 (가만히 서서 Q 눌러도 대쉬해 나가게 함)
                if (input.sqrMagnitude > 0.01f)
                {
                    lastMoveDirection = input.normalized;
                }

                HandleDirectionRotation();
            }

            // ★ Q 키 입력 체크 (대쉬 중이거나 일반 공격 중일 땐 차단하는 로직은 유지)
            if (Keyboard.current.qKey.wasPressedThisFrame && canMove && !isAttacking && !isDashing)
            {
                if (Time.time >= nextQAttackTime)
                {
                    nextQAttackTime = Time.time + qCooldown;
                    StartCoroutine(DashRoutine());
                }
            }

            // E 키 입력 체크
            if (Keyboard.current.eKey.wasPressedThisFrame && canMove && !isAttacking && !isDashing)
            {
                if (Time.time >= nextEAttackTime)
                {
                    nextEAttackTime = Time.time + eCooldown;
                    StartCoroutine(AttackRoutine(eAttackPrefab, eDamage));
                }
            }

            // R 키 입력 체크
            if (Keyboard.current.rKey.wasPressedThisFrame && canMove && !isAttacking && !isDashing)
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

        // ★ 대쉬 중일 때는 걷기 애니메이션 프레임 계산 생략
        if (isDashing) return;

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
        // ★ 대쉬 중일 때는 고유 물리 주행을 하므로 FixedUpdate 연산을 생략합니다.
        if (isDashing) return;

        // -------------------------------------------------------------
        // ★ [수정]: 조건문에서 isAttacking 차단을 삭제했습니다!
        // 이제 공격을 실행해도 속도(linearVelocity)가 Zero로 묶이지 않고 뚫고 지나갑니다.
        // -------------------------------------------------------------
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // 대쉬 루틴 안에서 잔상 생성을 호출하도록 수정
    private IEnumerator DashRoutine()
    {
        isDashing = true;

        // ★ 잔상 생성 시작
        StartCoroutine(DashGhostRoutine(dashDuration));

        rb.linearVelocity = lastMoveDirection * dashSpeed;
        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
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

    private IEnumerator AttackRoutine(GameObject attackPrefab, float skillDamage)
    {
        if (attackPrefab == null) yield break;

        isAttacking = true;

        // 💡 플레이어가 걸어다니면서 쏠 수 있게 공격 직후 멈춰 세우던 아래 물리 초기화 두 줄은 주석 처리했습니다.
        // rb.linearVelocity = Vector2.zero;
        // velocity = Vector2.zero;

        // 프리팹이 켜지기 전에 대미지를 먼저 배달합니다.
        PlayerAttack attackScript = attackPrefab.GetComponent<PlayerAttack>();
        if (attackScript != null)
        {
            attackScript.SetAttackDamage(skillDamage);
        }

        // 대미지 주입이 완료된 후, 프리팹을 활성화!
        attackPrefab.SetActive(true);

        Animator effectAnim = attackPrefab.GetComponent<Animator>();
        float attackDuration = 0.15f;

        if (effectAnim != null)
        {
            attackDuration = effectAnim.GetCurrentAnimatorStateInfo(0).length;
        }

        Light2D skillLight = attackPrefab.GetComponentInChildren<Light2D>();
        float elapsedTime = 0f;

        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / attackDuration;

            if (skillLight != null)
            {
                skillLight.intensity = Mathf.Sin(t * Mathf.PI) * 2f;
            }

            yield return null;
        }

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
       // if (collision.CompareTag("Respawn"))
      //  {
       //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
      //      return;
      //  }
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

    // 잔상을 지속적으로 생성하는 코루틴
    private IEnumerator DashGhostRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (ghostPrefab != null)
            {
                GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
                SpriteRenderer ghostSr = ghost.GetComponent<SpriteRenderer>();

                if (ghostSr != null)
                {
                    ghostSr.sprite = sr.sprite;
                    ghostSr.flipX = sr.flipX;
                    ghostSr.color = new Color(0.5f, 0.5f, 0.6f, 0.8f);
                }
            }

            yield return new WaitForSeconds(ghostInterval);
            // 대쉬 도중 프레임 연산 오차 방지를 위해 안전하게 누적 처리
            elapsed += ghostInterval;
        }
    }
}