using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPC : MonoBehaviour
{
    // 플레이어 근처 체크
    bool canTalk;

    // Y/N 입력 대기
    bool waitingChoice = false;

    // 구매 질문 출력 중인지
    bool isAsking = false;

    // ★ 추가: 결과 대사(성공/취소/돈부족)가 출력 중인지 체크
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

    [Header("판매할 직업 데이터")]
    public JobData sellJobData; // 인스펙터에서 전사, 마법사 등의 에셋을 넣습니다.

    public TextMeshProUGUI bubbleText;

    public TextMeshProUGUI nameText;

    // 첫 설명 여부
    bool explained = false;

    void Start()
    {
        bubblePanel.SetActive(false);
    }

    void Update()
    {
        // 플레이어 근처 아니면 종료
        if (!canTalk)
            return;

        // -----------------------------
        // ★ 추가: 결과 대사 출력 중일 때의 처리
        // -----------------------------
        if (isShowingResult)
        {
            // 결과 대사 출력이 완전히 끝났다면
            if (!DialogueManager.Instance.IsDialogue())
            {
                // 완전히 대화를 종료하고 상점 상태를 리셋합니다.
                isShowingResult = false;
                bubblePanel.SetActive(false);
                DialogueManager.Instance.SetWaitingInput(false);
            }
            return; // 결과 대사 중에는 아래의 구매 관련 로직을 절대 실행하지 않음
        }

        // -----------------------------
        // 구매 질문 끝났는지 체크
        // -----------------------------
        if (isAsking)
        {
            // 대화 종료 시 (타이핑이 모두 끝났을 때)
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
                BuyJob();
                return;
            }

            // N 키 (구매 취소)
            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                waitingChoice = false;
                DialogueManager.Instance.SetWaitingInput(false);

                // 결과 대사 상태로 변경
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

            // 이미 대화 중이면 종료
            if (DialogueManager.Instance.IsDialogue())
                return;

            // -----------------------------
            // 첫 설명
            // -----------------------------
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

            // -----------------------------
            // 구매 질문 시작
            // -----------------------------
            isAsking = true;

            // Y/N 대기 상태 설정
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

    // -----------------------------
    // 실제 구매 처리
    // -----------------------------
    // -----------------------------
    // 실제 구매 처리 (버그 수정 반영)
    // -----------------------------
    // -----------------------------
    // 실제 구매 처리 (돈 부족 버그 완벽 수정)
    // -----------------------------
    void BuyJob()
    {
        // 1. 현재 플레이어의 코인 양을 가져옵니다.
        int coin = GameDataManager.Instance.playerData.coin;

        // 2. [돈 부족 체크] 가진 돈이 직업 가격보다 적다면?
        if (coin < sellJobData.price)
        {
            isShowingResult = true;
            
            // "돈이 부족합니다" 대사 출력
            DialogueManager.Instance.ChangeText(noMoneyDialogue);
            
            // ★ 중요: 돈이 없으므로 아래의 코인 차감/직업 변경을 하지 못하도록 
            // 여기서 함수를 즉시 종료(return)합니다!
            return; 
        }

        // 3. [돈이 충분할 때만 실행되는 구간] 코인 차감
        GameDataManager.Instance.playerData.coin -= sellJobData.price;
        
        // 4. 플레이어에게 직업 데이터 전달 및 세이브 데이터에 직업 이름 기록
        PlayerController player = DialogueManager.Instance.player;
        if (player != null)
        {
            player.ChangeJob(sellJobData);
            GameDataManager.Instance.playerData.currentJob = sellJobData.jobName;
        }

        // 5. JSON 파일로 저장
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);

        // 6. 구매 완료 대사 출력
        isShowingResult = true;
        DialogueManager.Instance.ChangeText(successDialogue);
    }

    // -----------------------------
    // 플레이어 접근 및 멀어짐 (기존과 동일)
    // -----------------------------
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
            isShowingResult = false; // 리셋
            bubblePanel.SetActive(false);
            DialogueManager.Instance.SetWaitingInput(false);
        }
    }
}