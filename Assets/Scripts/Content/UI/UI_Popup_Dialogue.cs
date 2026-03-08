using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Popup_Dialogue : UI_Popup
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public GameObject namePanel;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private bool _isTyping = false;
    private Coroutine _typingCoroutine;
    [SerializeField] private DialogueManager.DialogueData _currentDialogueData;

    // CSV에서 Speaker 칸은 영어로 입력
    private Dictionary<string, string> _speakerNameMap = new Dictionary<string, string>()
    {
        { "Stapi", "스타피" },
        { "Player", "주인공" },
        { "Letter", "엄마의 편지" },
        // 계속 추가..
    };

    private void OnEnable()
    {
        if (Managers.Input != null)
        {
            Managers.Input.OnSubmitPressed += SkipCurrentDialogue; // 엔터키
            Managers.Input.OnInteractPressed += SkipCurrentDialogue; // 스페이스키
        }
    }

    private void OnDisable()
    {
        if (Managers.Input != null)
        {
            Managers.Input.OnSubmitPressed -= SkipCurrentDialogue;
            Managers.Input.OnInteractPressed -= SkipCurrentDialogue;
        }
    }

    public void SetDialogue(DialogueManager.DialogueData data)
    {
        _currentDialogueData = data;

        // Speaker 이름 설정
        if (string.IsNullOrEmpty(data.Speaker))
        {
            if (nameText != null) nameText.gameObject.SetActive(false);
            if (namePanel != null) namePanel.SetActive(false);
        }
        else
        {
            if (nameText != null) nameText.gameObject.SetActive(true);
            if (namePanel != null) namePanel.SetActive(true);

            // 딕셔너리에 영문 ID가 있으면 한글로, 없으면 엑셀에 적힌 원본 그대로 출력
            if (_speakerNameMap.TryGetValue(data.Speaker, out string localizedName))
                nameText.text = localizedName;
            else
                nameText.text = data.Speaker;
        }

        // 초상화(Portrait) 설정
        if (!string.IsNullOrEmpty(data.PortraitName))
        {
            Sprite loadedSprite = Managers.Resource.Load<Sprite>($"Portraits/{data.PortraitName}"); // Resources/Portraits/{이름}
            if (loadedSprite != null)
            {
                portraitImage.sprite = loadedSprite;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
                Debug.LogWarning($"[UI_Popup_Dialogue] 초상화 이미지를 찾을 수 없습니다: {data.PortraitName}");
            }
        }
        else
        {
            portraitImage.enabled = false;
        }

        // 대화 이벤트가 존재하면 이벤트 실행
        Managers.Dialogue.TriggerDialogueEvent(data.EventName);

        // 타이핑 효과 시작
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypingEffect(data.DialogueText));
    }

    /// <summary>
    /// 상호작용, 엔터키를 한번 더 눌러 타이핑을 스킵
    /// </summary>
    private void SkipCurrentDialogue()
    {
        if (_currentDialogueData == null) return;

        if (_isTyping)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
            dialogueText.text = _currentDialogueData.DialogueText;
            _isTyping = false;
        }
        else
        {
            Managers.Dialogue.ProcessNextDialogue();
        }
    }

    private IEnumerator TypingEffect(string text)
    {
        _isTyping = true;

        // 원본 텍스트를 넣고 렌더링 정보를 업데이트
        dialogueText.text = text;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            dialogueText.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }
}