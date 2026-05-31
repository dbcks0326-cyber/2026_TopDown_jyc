using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour
{
    bool canTalk;

    [Header("NPC 이름")]
    public string npcName;

    [TextArea]
    public string[] firstDialogue;

    [TextArea]
    public string[] secondDialogue;

    bool talkedBefore = false;

    [Header("말풍선")]
    public GameObject bubblePanel;
    public TextMeshProUGUI bubbleText;
    public TextMeshProUGUI nameText;

    // ★ 추가: 대화가 방금 끝났음을 기억하는 플래그
    private bool didDialogueEndThisFrame = false;

    void Start()
    {
        bubblePanel.SetActive(false);
    }

    void Update()
    {
        // 프레임 시작할 때 플래그 초기화
        didDialogueEndThisFrame = false;

        // -------------------------------------------------------------
        // 1. 대화가 끝났을 때 말풍선 패널을 닫아주는 로직
        // -------------------------------------------------------------
        if (bubblePanel.activeSelf && DialogueManager.Instance != null)
        {
            if (!DialogueManager.Instance.IsDialogue())
            {
                bubblePanel.SetActive(false); // 말풍선 끄기
                didDialogueEndThisFrame = true; // ★ 이번 프레임에 대화가 끝났음을 기록!
            }
        }

        // 플레이어 근처가 아니면 아래 대화 시작 로직 실행 안 함
        if (!canTalk)
            return;

        // -------------------------------------------------------------
        // 2. F 키 입력으로 대화 시작
        // -------------------------------------------------------------
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance == null)
                return;

            // 이미 대화 중이라면 중복 실행 방지
            if (DialogueManager.Instance.IsDialogue())
                return;

            // ★ 핵심 수정: 방금 대화가 끝난 프레임이라면 대화를 다시 시작하지 않고 튕겨냅니다.
            if (didDialogueEndThisFrame)
                return;

            // 이제 안전하게 처음 대화 시작
            if (talkedBefore)
            {
                DialogueManager.Instance.StartDialogue(
                    bubblePanel,
                    bubbleText,
                    nameText,
                    npcName,
                    secondDialogue
                );
            }
            else
            {
                DialogueManager.Instance.StartDialogue(
                    bubblePanel,
                    bubbleText,
                    nameText,
                    npcName,
                    firstDialogue
                );

                talkedBefore = true;
            }
        }
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

            if (bubblePanel != null)
            {
                bubblePanel.SetActive(false);
            }
        }
    }
}