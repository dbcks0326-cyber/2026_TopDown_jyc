using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public PlayerController player;
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;

    [Header("Typing")]
    public float typingSpeed = 0.05f;

    string[] currentLines;
    int currentIndex;

    bool isTyping;
    bool isDialogue;

    Coroutine typingCoroutine;

    void Awake()
    {
        Instance = this;

        player = FindFirstObjectByType<PlayerController>();

        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogue &&
     Keyboard.current.fKey.wasPressedThisFrame)
        { 
            NextLine();
        }
    }

    public void StartDialogue(string npcName, string[] lines)
    {
        Camera.main.GetComponent<FollowingCamera>().ZoomIn();
        player.canMove = false;

        isDialogue = true;

        dialoguePanel.SetActive(true);

        currentLines = lines;
        currentIndex = 0;

        nameText.text = npcName;

        StartTyping();
    }

    void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;

        dialogueText.text = "";

        string line = currentLines[currentIndex];

        foreach (char c in line)
        {
            dialogueText.text += c;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueText.text = currentLines[currentIndex];

            isTyping = false;

            return;
        }

        currentIndex++;

        if (currentIndex >= currentLines.Length)
        {
            EndDialogue();

            return;
        }

        StartTyping();
    }

    void EndDialogue()
    {
        Camera.main.GetComponent<FollowingCamera>().ZoomOut();
        isDialogue = false;

        dialoguePanel.SetActive(false);

        player.canMove = true;
    }

    public bool IsDialogue()
    {
        return isDialogue;
    }
}