using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class NPCController : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "Unknown NPC";
    public string startDialogueID = "";

    [Header("Special Events")]
    public UnityEvent OnInteractEvent;
    public UnityEvent OnDialogueEndEvent;

    private bool _isPlayerInRange = false;
    private bool _isInteracting = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void Interact()
    {
        // 범위 밖이거나 이미 대화 중이면 무시
        if (!_isPlayerInRange || _isInteracting) return;

        _isInteracting = true; // 대화 상태 진입

        SingletonManagers.Input.SetInputModeUI(true);
        OnInteractEvent?.Invoke();

        if (!string.IsNullOrEmpty(startDialogueID))
        {
            SingletonManagers.Dialogue.StartDialogue(startDialogueID);
            SingletonManagers.Dialogue.OnDialogueEnd += HandleDialogueEnd;
        }
        else
        {
            HandleDialogueEnd();
        }
    }

    private void HandleDialogueEnd()
    {
        SingletonManagers.Dialogue.OnDialogueEnd -= HandleDialogueEnd;

        OnDialogueEndEvent?.Invoke();

        SingletonManagers.Input.SetInputModeUI(false);

        _isInteracting = false; // 대화 상태 해제
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            SingletonManagers.Input.OnInteractPressed += Interact;
            Debug.Log($"[NPCController] {npcName}에게 상호작용 가능");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            SingletonManagers.Input.OnInteractPressed -= Interact;
            Debug.Log($"[NPCController] {npcName}에게 상호작용 불가능");
        }
    }
}