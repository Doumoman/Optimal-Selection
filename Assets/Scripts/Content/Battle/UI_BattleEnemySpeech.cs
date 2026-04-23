using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BattleEnemySpeech : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI speechText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject namePanel;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private string _fullText;
    private Action _onComplete;

    public void Show(string speaker, string text, Sprite portrait, Action onComplete = null)
    {
        _fullText = text;
        _onComplete = onComplete;

        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);

        if (nameText != null)
            nameText.text = speaker;

        if (namePanel != null)
            namePanel.SetActive(!string.IsNullOrEmpty(speaker));

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

        _typingCoroutine = StartCoroutine(CoTyping(text));
    }

    public void HideImmediate()
    {
        _isTyping = false;
        _fullText = string.Empty;
        _onComplete = null;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        if (speechText != null)
            speechText.text = string.Empty;

        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private IEnumerator CoTyping(string text)
    {
        _isTyping = true;

        speechText.text = text;
        speechText.ForceMeshUpdate();

        int totalVisibleCharacters = speechText.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            speechText.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }

    private void OnEnable()
    {
        if (SingletonManagers.Input != null)
        {
            SingletonManagers.Input.OnUISubmitPressed += OnSubmitPressed;
            SingletonManagers.Input.OnInteractPressed += OnSubmitPressed;
        }
    }

    private void OnDisable()
    {
        if (SingletonManagers.Input != null)
        {
            SingletonManagers.Input.OnUISubmitPressed -= OnSubmitPressed;
            SingletonManagers.Input.OnInteractPressed -= OnSubmitPressed;
        }
    }

    private void OnSubmitPressed()
    {
        bool visible = root != null ? root.activeSelf : gameObject.activeSelf;
        if (!visible) return;

        if (_isTyping)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            speechText.text = _fullText;
            speechText.ForceMeshUpdate();
            speechText.maxVisibleCharacters = speechText.textInfo.characterCount;
            _isTyping = false;
        }
        else
        {
            Action callback = _onComplete;
            HideImmediate();
            callback?.Invoke();
        }
    }
}