using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public PlayerController player;

    public Action onDialogueEnd;

    public static DialogueManager Instance;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.05f;

    string[] currentLines;

    int currentIndex;

    bool isTyping;

    bool isDialogue;

    Coroutine typingCoroutine;

    // 현재 사용 중인 말풍선
    GameObject currentBubblePanel;

    TextMeshProUGUI currentBubbleText;

    TextMeshProUGUI currentNameText;

    // Y/N 입력 대기 상태
    bool waitingInput = false;

    void Awake()
    {
        Instance = this;

        player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        // 대화 중 F 입력
        if (isDialogue && Keyboard.current.fKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    // -----------------------------
    // Y/N 입력 대기 설정
    // -----------------------------
    public void SetWaitingInput(bool value)
    {
        waitingInput = value;
    }

    // -----------------------------
    // 현재 대화 중인지
    // -----------------------------
    public bool IsDialogue()
    {
        return isDialogue;
    }

    // -----------------------------
    // 현재 타이핑 중인지
    // -----------------------------
    public bool IsTyping()
    {
        return isTyping;
    }

    // -----------------------------
    // 대화 시작
    // -----------------------------
    public void StartDialogue(
        GameObject bubblePanel,
        TextMeshProUGUI bubbleText,
        TextMeshProUGUI nameText,
        string npcName,
        string[] lines)
    {
        // 새로운 대화 시작 시 이전 이벤트 초기화 (안전장치)
        onDialogueEnd = null;

        currentBubblePanel = bubblePanel;
        currentBubbleText = bubbleText;
        currentNameText = nameText;

        // 카메라 확대
        Camera.main.GetComponent<FollowingCamera>().ZoomIn();

        // 플레이어 이동 막기
        player.canMove = false;

        isDialogue = true;

        // 말풍선 켜기
        currentBubblePanel.SetActive(true);

        currentLines = lines;
        currentIndex = 0;

        // 이름 표시
        currentNameText.text = npcName;

        StartTyping();
    }

    // -----------------------------
    // 현재 대화 텍스트만 변경 (버그 수정 완료)
    // -----------------------------
    public void ChangeText(string[] lines)
    {
        currentLines = lines;
        currentIndex = 0;

        // ★ 중요: 결과 대사를 출력할 때도 시스템이 대화 중(isDialogue)임을 인지해야 
        // 나중에 F키를 눌러 대화를 종료하고 움직일 수 있습니다.
        isDialogue = true;

        StartTyping();
    }

    // -----------------------------
    // 타이핑 시작
    // -----------------------------
    void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    // -----------------------------
    // 한 글자씩 출력
    // -----------------------------
    IEnumerator TypeText()
    {
        isTyping = true;
        currentBubbleText.text = "";

        string line = currentLines[currentIndex];

        foreach (char c in line)
        {
            currentBubbleText.text += c;

            // ★ 개선: 현재 글자가 '공백(띄어쓰기)'이 아닐 때만 소리를 냅니다.
            if (c != ' ' && Soundmanager.Instance != null)
            {
                Soundmanager.Instance.TextSound();
            }

            yield return new WaitForSeconds(typingSpeed);
            
        }

        isTyping = false;
    }

    // -----------------------------
    // 다음 대사
    // -----------------------------
    void NextLine()
    {
        // 타이핑 중이면 즉시 완성
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            currentBubbleText.text = currentLines[currentIndex];
            isTyping = false;
            return;
        }

        currentIndex++;

        // 마지막 대사일 때
        if (currentIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        StartTyping();
    }

    // -----------------------------
    // 대화 종료
    // -----------------------------
    // -----------------------------
    // 대화 종료 (매니저 역할에 충실하게 수정)
    // -----------------------------
    void EndDialogue()
    {
        isDialogue = false;

        // Y/N 선택 중이 아닐 때만 플레이어 상태를 복구합니다.
        if (!waitingInput)
        {
            // 카메라 복구
            Camera.main.GetComponent<FollowingCamera>().ZoomOut();

            // 플레이어 이동 가능
            if (player != null)
            {
                player.canMove = true;
            }

            // ❌ 기존에 여기서 currentBubblePanel.SetActive(false); 하던 것을 지웁니다!
            // 말풍선을 끄는 제어권은 대화를 주도한 NPC들의 Update나 Exit 로직에 맡깁니다.
        }

        // 이벤트 실행
        System.Action action = onDialogueEnd;
        onDialogueEnd = null;
        action?.Invoke();
    }
}