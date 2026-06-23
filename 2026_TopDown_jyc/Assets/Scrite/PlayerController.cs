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
    // ⭐ [추가]: 아이템으로 증가할 공격력 보너스 변수
    private float equipmentDamageBonus = 0f;

    private itemso equippedItem = null;

    [Header("대쉬 잔상 설정")]
    [SerializeField] private GameObject ghostPrefab;
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
    private Vector2 lastMoveDirection = Vector2.down;

    [Header("모든 직업 데이터 리스트")]
    public List<JobData> allJobs;

    private bool isHurt = false;

    [Header("Q スキル設定 (대쉬)")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float qCooldown = 3f;
    private float nextQAttackTime = 0f;
    private bool isDashing = false;

    [Header("E 스킬 설정 (기본공격)")]
    [SerializeField] private GameObject eAttackPrefab;
    [SerializeField] private float eDamage = 10f; // 기본 데미지
    [SerializeField] private float eCooldown = 0.3f;
    private float nextEAttackTime = 0f;

    [Header("R 스킬 설정 (궁극기)")]
    [SerializeField] private GameObject rAttackPrefab;
    [SerializeField] private float rDamage = 40f; // 기본 데미지
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

    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;

    void Start()
    {
        string savedJobName = GameDataManager.Instance.playerData.currentJob;
        JobData savedJob = allJobs.Find(job => job.jobName == savedJobName);
        if (savedJob != null)
        {
            ChangeJob(savedJob);
        }

        string savedItemName = GameDataManager.Instance.playerData.equippedItemName;
        if (!string.IsNullOrEmpty(savedItemName))
        {
            itemso savedItem = GameDataManager.Instance.allItemSOList.Find(item => item.itemName == savedItemName);
            if (savedItem != null)
            {
                EquipItem(savedItem);
                Debug.Log($"✨ [씬 이동 완료] 데이터 매니저를 통해 {savedItem.itemName} 자동 재장착 및 스탯 적용 완료!");
            }
            else
            {
                Debug.LogWarning($"⚠️ [장착 실패] 데이터 매니저 리스트에서 '{savedItemName}' 아이템을 찾을 수 없습니다.");
            }
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
            if (canMove && !isDashing)
            {
                float moveX = 0f;
                float moveY = 0f;

                if (Keyboard.current.aKey.isPressed) moveX = -1f;
                if (Keyboard.current.dKey.isPressed) moveX = 1f;
                if (Keyboard.current.sKey.isPressed) moveY = -1f;
                if (Keyboard.current.wKey.isPressed) moveY = 1f;

                input = new Vector2(moveX, moveY);
                velocity = input.normalized * (moveSpeed + equipmentSpeedBonus);

                if (input.sqrMagnitude > 0.01f)
                {
                    lastMoveDirection = input.normalized;
                }

                HandleDirectionRotation();
            }

            if (Keyboard.current.qKey.wasPressedThisFrame && canMove && !isAttacking && !isDashing)
            {
                if (Time.time >= nextQAttackTime)
                {
                    nextQAttackTime = Time.time + qCooldown;
                    StartCoroutine(DashRoutine());
                }
            }

            // ⭐ [수정]: 스킬을 쓸 때 기본 데미지(eDamage)에 아이템 보너스 데미지를 더해서 보냅니다!
            if (Keyboard.current.eKey.wasPressedThisFrame && canMove && !isAttacking && !isDashing)
            {
                if (Time.time >= nextEAttackTime)
                {
                    nextEAttackTime = Time.time + eCooldown;
                    float finalDamage = eDamage + equipmentDamageBonus; // 최종 데미지 계산
                    StartCoroutine(AttackRoutine(eAttackPrefab, finalDamage));
                }
            }

            // ⭐ [수정]: 궁극기도 마찬가지로 아이템 보너스 데미지를 합산합니다!
            if (Keyboard.current.rKey.wasPressedThisFrame && canMove && !isAttacking && !isDashing)
            {
                if (Time.time >= nextRAttackTime)
                {
                    nextRAttackTime = Time.time + rCooldown;
                    float finalDamage = rDamage + equipmentDamageBonus; // 최종 데미지 계산
                    StartCoroutine(AttackRoutine(rAttackPrefab, finalDamage));
                }
            }
        }

        UpdateCooldownUI(nextQAttackTime, qCooldown, qCooldownImage);
        UpdateCooldownUI(nextEAttackTime, eCooldown, eCooldownImage);
        UpdateCooldownUI(nextRAttackTime, rCooldown, rCooldownImage);

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
        if (isDashing) return;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
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

        PlayerAttack attackScript = attackPrefab.GetComponent<PlayerAttack>();
        if (attackScript != null)
        {
            attackScript.SetAttackDamage(skillDamage);
        }

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
        Debug.Log($"[인벤토리 체크] 무언가와 부딪힘! 부딪힌 오브젝트 이름: {collision.gameObject.name}, 태그: {collision.tag}");

        if (collision.CompareTag("Coin"))
        {
            Debug.Log($"[인벤토리 체크] 'Coin' 태그 인식 성공! itemOB 컴포넌트를 가져옵니다.");

            itemOB coinItem = collision.GetComponent<itemOB>();
            if (coinItem != null)
            {
                string itemName = coinItem.GetItemName();
                int coinAmount = coinItem.GetCoin();
                Debug.Log($"[인벤토리 체크] 아이템 정보 획득 성공 -> 이름: {itemName}, 코인 수량: {coinAmount}");

                GameDataManager.Instance.playerData.collectedItems.Add(itemName);
                GameDataManager.Instance.playerData.coin += coinAmount;

                Debug.Log($"[인벤토리 체크] 데이터 매니저 저장 완료! 현재 소지품 개수: {GameDataManager.Instance.playerData.collectedItems.Count}개");

                UI_ImageInventory imgInv = FindFirstObjectByType<UI_ImageInventory>();
                if (imgInv != null)
                {
                    Debug.Log("[인벤토리 체크] UI_ImageInventory 스크립트를 찾았습니다! UI를 새로고침합니다.");
                    imgInv.UpdateInventoryUI();
                }
                else
                {
                    Debug.LogWarning("[인벤토리 체크 경고] 씬에 UI_ImageInventory 오브젝트가 없거나 꺼져있습니다!");
                }

                Destroy(collision.gameObject);
            }
            else
            {
                Debug.LogError($"[인벤토리 에러] {collision.gameObject.name}에 'itemOB' 스크립트가 붙어있지 않습니다!");
            }
        }

        if (collision.CompareTag("Finish"))
        {
            GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
            collision.GetComponent<LevelObject>().MoveToNextLevel();
        }
    }

    // ⭐ [수정]: 아이템 장착 시 공격력 보너스 수치도 동기화합니다.
    public void EquipItem(itemso newItem)
    {
        if (equippedItem != null)
        {
            UnequipItem();
        }

        equippedItem = newItem;
        equipmentSpeedBonus = newItem.speedBonus;
        equipmentDamageBonus = newItem.attackBonus; // 👈 아이템 데미지 보너스 주입!

        GameDataManager.Instance.playerData.equippedItemName = newItem.itemName;
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);

        Debug.Log($"⚔️ [플레이어 장착 완료] {newItem.itemName} 장착! 이속: +{equipmentSpeedBonus}, 공증: +{equipmentDamageBonus}");
    }

    // ⭐ [수정]: 아이템 해제 시 공격력 보너스도 0으로 초기화합니다.
    public void UnequipItem()
    {
        if (equippedItem == null) return;

        Debug.Log($"🛡️ [플레이어 장착 해제] {equippedItem.itemName} 해제! 스탯이 원상복구됩니다.");

        equipmentSpeedBonus = 0f;
        equipmentDamageBonus = 0f; // 👈 공격력 보너스 복구!
        equippedItem = null;

        GameDataManager.Instance.playerData.equippedItemName = "";
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
    }

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
            elapsed += ghostInterval;
        }
    }
}