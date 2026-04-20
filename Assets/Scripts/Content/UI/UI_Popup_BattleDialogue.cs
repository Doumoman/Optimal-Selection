using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Popup_BattleDialogue : UI_Popup
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public GameObject namePanel;

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    private bool _isTyping;
    private Coroutine _typingCoroutine;
    private Action _onComplete;
    private string _fullText;

    public override void Init()
    {
        base.Init();
    }

    private void OnEnable()
    {
        if (SingletonManagers.Input != null)
        {
            SingletonManagers.Input.OnSubmitPressed += OnSubmitPressed;
            SingletonManagers.Input.OnInteractPressed += OnSubmitPressed;
        }
    }

    private void OnDisable()
    {
        if (SingletonManagers.Input != null)
        {
            SingletonManagers.Input.OnSubmitPressed -= OnSubmitPressed;
            SingletonManagers.Input.OnInteractPressed -= OnSubmitPressed;
        }
    }

    public void SetDialogue(string speaker, string text, Sprite portrait, Action onComplete = null)
    {
        _onComplete = onComplete;
        _fullText = text;

        if (string.IsNullOrEmpty(speaker))
        {
            if (nameText != null) nameText.gameObject.SetActive(false);
            if (namePanel != null) namePanel.SetActive(false);
        }
        else
        {
            if (nameText != null) nameText.gameObject.SetActive(true);
            if (namePanel != null) namePanel.SetActive(true);
            nameText.text = speaker;
        }

        if (portraitImage != null)
        {
            if (portrait != null)
            {
                portraitImage.enabled = true;
                portraitImage.sprite = portrait;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypingEffect(text));
    }

    private IEnumerator TypingEffect(string text)
    {
        _isTyping = true;

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

    private void OnSubmitPressed()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_isTyping)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            dialogueText.text = _fullText;
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
            _isTyping = false;
        }
        else
        {
            _onComplete?.Invoke();
        }
    }
}