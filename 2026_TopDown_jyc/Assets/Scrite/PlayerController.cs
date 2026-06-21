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

    private float equipmentSpeedBonus = 0f;

    private itemso equippedItem = null;

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
        // 1. [순서 보장 1] 기존 직업 데이터 불러오기 로직 (기본 moveSpeed가 먼저 결정됨)
        string savedJobName = GameDataManager.Instance.playerData.currentJob;
        JobData savedJob = allJobs.Find(job => job.jobName == savedJobName);
        if (savedJob != null)
        {
            ChangeJob(savedJob);
        }

        // ───────────────────────────────────────────────────────────
        // ★ [중요 수정]: 인벤토리 UI 안 거치고, GameDataManager에서 직접 템 꺼내서 장착!
        // ───────────────────────────────────────────────────────────
        string savedItemName = GameDataManager.Instance.playerData.equippedItemName;
        if (!string.IsNullOrEmpty(savedItemName))
        {
            // 중앙 데이터 매니저가 로드해둔 올 아이템 리스트에서 "er"을 찾습니다.
            itemso savedItem = GameDataManager.Instance.allItemSOList.Find(item => item.itemName == savedItemName);
            if (savedItem != null)
            {
                // 찾았다면 즉시 장착해서 equipmentSpeedBonus 수치를 정상 주입!
                EquipItem(savedItem);
                Debug.Log($"✨ [씬 이동 완료] 데이터 매니저를 통해 {savedItem.itemName} 자동 재장착 및 스탯 적용 완료!");
            }
            else
            {
                Debug.LogWarning($"⚠️ [장착 실패] 데이터 매니저 리스트에서 '{savedItemName}' 아이템을 찾을 수 없습니다.");
            }
        }

        // 2. 기존 체력 및 UI 초기화 로직
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
            // Update() 함수 내부의 이동 입력 처리 부분입니다.
            if (canMove && !isDashing)
            {
                float moveX = 0f;
                float moveY = 0f;

                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                if (Keyboard.current.dKey.isPressed) moveX = 1f;
                if (Keyboard.current.sKey.isPressed) moveY = -1f;
                if (Keyboard.current.wKey.isPressed) moveY = 1f;

                input = new Vector2(moveX, moveY);

                // ❌ 기존 코드: velocity = input.normalized * moveSpeed;
                // ───────────────────────────────────────────────────────────
                // ★ [수정]: 기본 속도에 장비 보너스 속도를 더해서 최종 속도를 냅니다!
                // ───────────────────────────────────────────────────────────
                velocity = input.normalized * (moveSpeed + equipmentSpeedBonus);

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
        // [로그 1] 충돌이 일어났을 때 무조건 실행되는 로그
        Debug.Log($"[인벤토리 체크] 무언가와 부딪힘! 부딪힌 오브젝트 이름: {collision.gameObject.name}, 태그: {collision.tag}");

        if (collision.CompareTag("Coin"))
        {
            // [로그 2] 태그가 "Coin"인 것을 정상적으로 인식했을 때
            Debug.Log($"[인벤토리 체크] 'Coin' 태그 인식 성공! itemOB 컴포넌트를 가져옵니다.");

            itemOB coinItem = collision.GetComponent<itemOB>();
            if (coinItem != null)
            {
                // [로그 3] itemOB 컴포넌트까지 성공적으로 가져왔을 때
                string itemName = coinItem.GetItemName();
                int coinAmount = coinItem.GetCoin();
                Debug.Log($"[인벤토리 체크] 아이템 정보 획득 성공 -> 이름: {itemName}, 코인 수량: {coinAmount}");

                // 1. 데이터 매니저 리스트에 아이템 이름 저장 및 코인 증가 (기존 로직)
                GameDataManager.Instance.playerData.collectedItems.Add(itemName);
                GameDataManager.Instance.playerData.coin += coinAmount;

                // ★ 추가된 디버그 로그: 데이터 매니저에 잘 들어갔는지 확인
                Debug.Log($"[인벤토리 체크] 데이터 매니저 저장 완료! 현재 소지품 개수: {GameDataManager.Instance.playerData.collectedItems.Count}개");

                // 2. 씬에 존재하는 이미지 인벤토리 UI를 찾아 실시간으로 새로고침합니다.
                UI_ImageInventory imgInv = FindFirstObjectByType<UI_ImageInventory>();
                if (imgInv != null)
                {
                    Debug.Log("[인벤토리 체크] UI_ImageInventory 스크립트를 찾았습니다! UI를 새로고침합니다.");
                    imgInv.UpdateInventoryUI();
                }
                else
                {
                    // UI 스크립트를 못 찾으면 경고를 띄웁니다.
                    Debug.LogWarning("[인벤토리 체크 경고] 씬에 UI_ImageInventory 오브젝트가 없거나 꺼져있습니다! (I 키를 눌러 켜는 구조라면 정상일 수 있음)");
                }

                // 3. 먹은 아이템 오브젝트 파괴 (기존 로직)
                Destroy(collision.gameObject);
            }
            else
            {
                // [에러 로그] 오브젝트에 itemOB 스크립트가 없을 때
                Debug.LogError($"[인벤토리 에러] {collision.gameObject.name}에 'itemOB' 스크립트(컴포넌트)가 붙어있지 않습니다!");
            }
        }

        if (collision.CompareTag("Finish"))
        {
            GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
    }

    // ───────────────────────────────────────────────────────────
    // ★ [추가]: 인벤토리 슬롯에서 호출할 실질적인 장착 및 해제 함수
    // ───────────────────────────────────────────────────────────
    public void EquipItem(itemso newItem)
    {
        if (equippedItem != null)
        {
            UnequipItem();
        }

        equippedItem = newItem;
        equipmentSpeedBonus = newItem.speedBonus;

        // ★ 데이터 매니저에 현재 장착한 아이템 이름 기록하고 저장!
        GameDataManager.Instance.playerData.equippedItemName = newItem.itemName;
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);

        Debug.Log($"⚔️ [플레이어 장착 완료] {newItem.itemName} 장착! 보너스 이속: +{equipmentSpeedBonus}");
    }

    public void UnequipItem()
    {
        if (equippedItem == null) return;

        Debug.Log($"🛡️ [플레이어 장착 해제] {equippedItem.itemName} 해제! 스탯이 원상복구됩니다.");

        equipmentSpeedBonus = 0f;
        equippedItem = null;

        // ★ 데이터 매니저에서 장착한 아이템 이름 지우고 저장!
        GameDataManager.Instance.playerData.equippedItemName = "";
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
    }

    // ★ [추가 된 꿀함수]: UI 슬롯에서 지금 이 아이템이 장착된 상태인지 편하게 알아보기 위해 사용
    public itemso GetEquippedItem()
    {
        return equippedItem;
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