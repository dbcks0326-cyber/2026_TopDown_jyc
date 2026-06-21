using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPC : MonoBehaviour
{
    // ★ 추가: 상점의 종류를 선택하는 열거형(Enum)
    public enum ShopType { SellJob, SellItem }

    [Header("★ 상점 타입 설정")]
    public ShopType shopType = ShopType.SellJob; // 인스펙터에서 직업을 팔지, 아이템을 팔지 고릅니다.

    [Header("판매할 상품 데이터 (타입에 맞게 채워주세요)")]
    public JobData sellJobData; // 직업 상점일 때 채우기 (전사, 기사 등)
    public itemso sellItemData; // 아이템 상점일 때 채우기 (er, 신발 등)

    // 플레이어 근처 체크
    bool canTalk;

    // Y/N 입력 대기
    bool waitingChoice = false;

    // 구매 질문 출력 중인지
    bool isAsking = false;

    // 결과 대사(성공/취소/돈부족)가 출력 중인지 체크
    bool isShowingResult = false;

    [Header("상인 이름")]
    public string npcName = "상인";

    [Header("첫 설명 대사")]
    [TextArea]
    public string[] introDialogue;

    [Header("구매 질문")]
    [TextArea]
    public string[] askBuyDialogue;

    [Header("돈 부족 대사")]
    [TextArea]
    public string[] noMoneyDialogue;

    [Header("구매 완료 대사")]
    [TextArea]
    public string[] successDialogue;

    [Header("구매 취소 대사")]
    [TextArea]
    public string[] cancelDialogue;

    [Header("말풍선")]
    public GameObject bubblePanel;

    public TextMeshProUGUI bubbleText;
    public TextMeshProUGUI nameText;

    // 첫 설명 여부
    bool explained = false;

    void Start()
    {
        if (bubblePanel != null)
            bubblePanel.SetActive(false);
    }

    void Update()
    {
        // 플레이어 근처 아니면 종료
        if (!canTalk)
            return;

        // -----------------------------
        // 결과 대사 출력 중일 때의 처리
        // -----------------------------
        if (isShowingResult)
        {
            if (!DialogueManager.Instance.IsDialogue())
            {
                isShowingResult = false;
                bubblePanel.SetActive(false);
                DialogueManager.Instance.SetWaitingInput(false);
            }
            return;
        }

        // -----------------------------
        // 구매 질문 끝났는지 체크
        // -----------------------------
        if (isAsking)
        {
            if (!DialogueManager.Instance.IsDialogue())
            {
                isAsking = false;
                waitingChoice = true;

                // 선택 문구 추가
                bubbleText.text += "\n\nY : 구매\nN : 취소";
            }
        }

        // -----------------------------
        // Y / N 선택
        // -----------------------------
        if (waitingChoice)
        {
            // Y 키 (구매 확정)
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                waitingChoice = false;
                DialogueManager.Instance.SetWaitingInput(false);
                ProcessPurchase(); // 직업/아이템을 구분하여 구매 연산 처리
                return;
            }

            // N 키 (구매 취소)
            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                waitingChoice = false;
                DialogueManager.Instance.SetWaitingInput(false);

                isShowingResult = true;
                DialogueManager.Instance.ChangeText(cancelDialogue);
                return;
            }

            return;
        }

        // -----------------------------
        // F 키 입력
        // -----------------------------
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance == null)
                return;

            if (DialogueManager.Instance.IsDialogue())
                return;

            // 첫 설명
            if (!explained)
            {
                DialogueManager.Instance.StartDialogue(
                    bubblePanel,
                    bubbleText,
                    nameText,
                    npcName,
                    introDialogue
                );

                explained = true;
                return;
            }

            // 구매 질문 시작
            isAsking = true;
            DialogueManager.Instance.SetWaitingInput(true);

            DialogueManager.Instance.StartDialogue(
                bubblePanel,
                bubbleText,
                nameText,
                npcName,
                askBuyDialogue
            );
        }
    }

    // ───────────────────────────────────────────────────────────
    // ★ [핵심 개조]: 상점 타입에 따라 가격 책정 및 재화 차감 후 지급 처리
    // ───────────────────────────────────────────────────────────
    void ProcessPurchase()
    {
        int currentCoin = GameDataManager.Instance.playerData.coin;
        int price = 0;

        // 1. 상점 타입에 따라 가격 책정 및 데이터 예외 확인
        if (shopType == ShopType.SellJob)
        {
            if (sellJobData == null)
            {
                Debug.LogError($"🚨 [{npcName}] 직업 상점인데 SellJobData가 비어있습니다!");
                return;
            }
            price = sellJobData.price;
        }
        else if (shopType == ShopType.SellItem)
        {
            if (sellItemData == null)
            {
                Debug.LogError($"🚨 [{npcName}] 아이템 상점인데 SellItemData가 비어있습니다!");
                return;
            }
            // ※ itemso 스크립트에 가격 변수명이 다를 경우(예: itemPrice 등) 해당 변수명으로 변경하세요.
            price = sellItemData.price;
        }

        // 2. 돈 부족 체크
        if (currentCoin < price)
        {
            isShowingResult = true;
            DialogueManager.Instance.ChangeText(noMoneyDialogue);
            return;
        }

        // 3. 돈이 충분하므로 코인 차감
        GameDataManager.Instance.playerData.coin -= price;

        // 4. 상점 타입별 보상 지급 처리
        if (shopType == ShopType.SellJob)
        {
            // 직업 변경 및 기록
            PlayerController player = DialogueManager.Instance.player;
            if (player != null)
            {
                player.ChangeJob(sellJobData);
                GameDataManager.Instance.playerData.currentJob = sellJobData.jobName;
            }
            Debug.Log($"🛒 [{npcName}] 직업 구매 완료: {sellJobData.jobName}, 차감 코인: {price}");
        }
        else if (shopType == ShopType.SellItem)
        {
            // 인벤토리 수집 목록 리스트에 아이템 이름 추가!
            if (!GameDataManager.Instance.playerData.collectedItems.Contains(sellItemData.itemName))
            {
                GameDataManager.Instance.playerData.collectedItems.Add(sellItemData.itemName);
            }

            // 인게임에 인벤토리 UI가 켜져 있다면 바로 아이템 아이콘이 뜨도록 동기화
            UI_ImageInventory imgInv = FindFirstObjectByType<UI_ImageInventory>();
            if (imgInv != null)
            {
                imgInv.UpdateInventoryUI();
            }
            Debug.Log($"🛒 [{npcName}] 아이템 구매 완료: {sellItemData.itemName}, 차감 코인: {price}");
        }

        // 5. JSON 데이터 저장 고정
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);

        // 6. 완료 대사 출력
        isShowingResult = true;
        DialogueManager.Instance.ChangeText(successDialogue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canTalk = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canTalk = false;
            waitingChoice = false;
            isAsking = false;
            isShowingResult = false;
            if (bubblePanel != null)
                bubblePanel.SetActive(false);
            DialogueManager.Instance.SetWaitingInput(false);
        }
    }
}