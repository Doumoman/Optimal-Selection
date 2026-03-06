using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events; // UnityEvent 사용을 위해 추가

[RequireComponent(typeof(Collider2D))]
public class NPCController : MonoBehaviour
{
    [Header("NPC Info")]
    [Tooltip("이 NPC의 고유 이름 (디버깅/식별용)")]
    public string npcName = "Unknown NPC";

    [Tooltip("상호작용 시 실행할 대화의 첫 ID (빈칸이면 대화를 실행하지 않음)")]
    public string startDialogueID = "";

    [Header("Special Events")]
    [Tooltip("말을 걸었을 때 대화와 함께, 혹은 대화 대신 실행될 특수 이벤트")]
    public UnityEvent OnInteractEvent;

    [Tooltip("대화가 완전히 종료된 직후에 실행될 특수 이벤트")]
    public UnityEvent OnDialogueEndEvent;

    [Header("Interaction Settings")]
    [Tooltip("플레이어가 상호작용 가능한 거리 내에 있는지 여부")]
    private bool _isPlayerInRange = false;

    private void Awake()
    {
        // 2D 충돌체 설정
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    /// <summary>
    /// 플레이어가 상호작용을 시도했을 때 실행되는 로직
    /// </summary>
    public void Interact()
    {
        if (!_isPlayerInRange) return;

        Managers.Input.SetInputModeUI(true); // 대화 빠르게 스킵 기능을 넣기 위해
        OnInteractEvent?.Invoke();

        if (!string.IsNullOrEmpty(startDialogueID))
        {
            // Managers.Dialogue.StartDialogue(startDialogueID);
            Managers.Dialogue.OnDialogueEnd += HandleDialogueEnd;
        }
        else
        {
            HandleDialogueEnd();
        }
    }

    /// <summary>
    /// 대화가 종료되었을 때 (또는 대화가 없는 경우 상호작용 직후) 호출됨
    /// </summary>
    private void HandleDialogueEnd()
    {
        
        if (!string.IsNullOrEmpty(startDialogueID))
        {
            Managers.Dialogue.OnDialogueEnd -= HandleDialogueEnd;
        }

        OnDialogueEndEvent?.Invoke();
        Managers.Input.SetInputModeUI(false);// 대화 빠르게 스킵 기능을 넣기 위해
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            Managers.Input.OnInteractPressed += Interact;
            Debug.Log($"[NPCController] {npcName}에게 상호작용 가능");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            Managers.Input.OnInteractPressed -= Interact;
            Debug.Log($"[NPCController] {npcName}에게 상호작용 불가능");
        }
    }

    private void OnDisable()
    {
        if (_isPlayerInRange && Managers.Input != null)
        {
            Managers.Input.OnInteractPressed -= Interact;
            _isPlayerInRange = false;
        }
    }
}