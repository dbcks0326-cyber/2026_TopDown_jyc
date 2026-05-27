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

    void Update()
    {
        if (canTalk &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 이미 대화 중이면 새 대화 시작 금지
            if (DialogueManager.Instance.IsDialogue())
                return;

            if (talkedBefore)
            {
                DialogueManager.Instance.StartDialogue(
                    npcName,
                    secondDialogue
                );
            }
            else
            {
                DialogueManager.Instance.StartDialogue(
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
        }
    }
}