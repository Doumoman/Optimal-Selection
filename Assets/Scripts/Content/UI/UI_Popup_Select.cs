using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Popup_Select : UI_Popup
{
    [Header("References")]
    [SerializeField] private RectTransform _popupPanel;
    [SerializeField] private Button[] _selectButtons;
    [SerializeField] private Sprite _selectSprite;
    [SerializeField] private Sprite _nonSelectSprite;

    [Header("Animation Settings")]
    [Tooltip("애니메이션이 완료되는 시간 (초)")]
    [SerializeField] private float _animDuration = 0.4f;

    [Tooltip("팝업이 등장했을 때 화면에 보일 목표 Y 좌표 (보통 0)")]
    [SerializeField] private float _targetY = 0f;

    [Tooltip("팝업이 숨겨져 있을 때의 시작 Y 좌표 (화면 아래)")]
    [SerializeField] private float _offScreenY = -1200f;

    [Tooltip("등장할 때의 연출 방식")]
    [SerializeField] private Ease _showEase = Ease.OutBack;

    [Tooltip("퇴장할 때의 연출 방식")]
    [SerializeField] private Ease _hideEase = Ease.InBack;

    private int _selectedIndex = 0;
    private int _activeSelectionCount = 0;
    private Tween _tween;
    private Action<int> _onSelectedCallback;

    private bool _isAnimating = false;

    private void Awake()
    {
        if (_popupPanel == null)
        {
            _popupPanel = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// 선택지 개수, 선택지 텍스트 설정 및 선택 정보 넘겨줌
    /// </summary>
    public void Setup(string[] choices, Action<int> onSelected)
    {
        _onSelectedCallback = onSelected;
        _activeSelectionCount = choices.Length;
        _selectedIndex = 0;

        if (_popupPanel != null)
        {
            _popupPanel.anchoredPosition = new Vector2(_popupPanel.anchoredPosition.x, _offScreenY);
        }

        // 전달받은 데이터 개수만큼 버튼 활성화 및 텍스트 설정
        for (int i = 0; i < _selectButtons.Length; i++)
        {
            if (i < _activeSelectionCount)
            {
                _selectButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI btnText = _selectButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = choices[i];
            }
            else
            {
                _selectButtons[i].gameObject.SetActive(false);
            }
        }

        UpdateSelectionUI();
        ShowPopup();

        Debug.Log("Set up!");
    }

    private void ShowPopup()
    {
        // 이미 실행 중인 애니메이션이 있다면 겹치지 않게 취소
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
        }

        _isAnimating = true;

        _tween = _popupPanel.DOAnchorPosY(_targetY, _animDuration)
            .SetEase(_showEase)
            .OnComplete(() => _isAnimating = false)
            .SetUpdate(true);
    }

    private void HidePopup(Action onComplete = null)
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
        }

        _isAnimating = true;
        _tween = _popupPanel.DOAnchorPosY(_offScreenY, _animDuration)
            .SetEase(_hideEase)
            .OnComplete(() =>
            {
                _isAnimating = false;

                SingletonManagers.UI.ClosePopupUI(this);
                onComplete?.Invoke();
            })
            .SetUpdate(true);
    }

    public override void OnInput(Vector2 direction)
    {
        if (_isAnimating || _activeSelectionCount == 0) return;

        if (direction.y > 0.1f)
        {
            _selectedIndex--;
            if (_selectedIndex < 0) _selectedIndex = _activeSelectionCount - 1;
            UpdateSelectionUI();
        }
        else if (direction.y < -0.1f)
        {
            _selectedIndex++;
            if (_selectedIndex >= _activeSelectionCount) _selectedIndex = 0;
            UpdateSelectionUI();
        }
    }

    public override void OnSubmit()
    {
        if (_isAnimating || _activeSelectionCount == 0) return;

        _selectButtons[_selectedIndex].onClick.Invoke();

        HidePopup(() =>
        {
            _onSelectedCallback?.Invoke(_selectedIndex);
        });
    }

    public override void OnCancel() { }

    private void UpdateSelectionUI()
    {
        if (_activeSelectionCount == 0) return;

        for (int i = 0; i < _activeSelectionCount; i++)
        {
            if (i == _selectedIndex)
            {
                _selectButtons[i].image.sprite = _selectSprite;
            }
            else
            {
                _selectButtons[i].image.sprite = _nonSelectSprite;
            }
        }
    }

    private void OnDestroy()
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
        }
    }
}