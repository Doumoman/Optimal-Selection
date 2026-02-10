using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Popup_Menu : UI_Popup
{
    [SerializeField] private Transform menuPanel;
    [SerializeField] private Transform btnPanel;
    [SerializeField] private Transform textPanel;

    [Header("Stats Info")]
    [SerializeField] private TextMeshProUGUI txtLocation;
    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtGold;

    [Header("Button Info")]
    [SerializeField] private Button[] menuButtons;

    [Header("Cursor Info")]
    public RectTransform cursor;
    public Vector2 cursorOffset = new Vector2(10f, 0f);

    private int _currentIndex = 0;
    private bool _init = false;
    private bool _canInput = false;
    public override void Init()
    {
        if(_init) return;
        base.Init();
        _init = true;

        _currentIndex = 0;

        _canInput = false;
        StartCoroutine(CoInitCursorPosition());

        // 버튼 이벤트 추가
        menuButtons[0].onClick.AddListener(ShowInventoryPopup);
    }

    private void Start()
    {
        Init();
    }

    public override void OnInput(Vector2 dir)
    {
        if (!_canInput) return;

        if (dir.x < -0.5f) // 왼쪽
        {
            _currentIndex = (_currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
            UpdateCursor();
        }
        else if (dir.x > 0.5f) // 오른쪽
        {
            _currentIndex = (_currentIndex + 1) % menuButtons.Length;
            UpdateCursor();
        }
    }

    public override void OnSubmit()
    {
        if (!_canInput) return;

        // 엔터키 입력 처리
        if (menuButtons != null && _currentIndex < menuButtons.Length)
        {
            if (menuButtons[_currentIndex] != null)
            {
                menuButtons[_currentIndex].onClick.Invoke();
            }
        }
    }

    public override void OnCancel()
    {
        if (!_canInput) return;
        // ESC 입력 처리 (팝업 닫기)
        Managers.UI.ClosePopupUI();
    }

    private void ShowInventoryPopup()
    {
        var popup = Managers.UI.ShowPopupUI<UI_Popup_Inventory>("UI_Popup_Inventory");
        popup.transform.SetParent(btnPanel.transform, false);
    }

    private IEnumerator CoInitCursorPosition()
    {
        // UI 요소(버튼 등)가 Layout Group(Horizontal/Vertical Layout Group) 안에 있을 경우, 계산을 늦게하기 때문에 한 프레임 기다림
        yield return null;
        UpdateCursor();

        yield return new WaitForSeconds(0.1f); // 바로 입력되는 것 방지
        _canInput = true;
    }
    private void UpdateCursor()
    {
        if (menuButtons == null || menuButtons.Length <= _currentIndex || cursor == null)
            return;

        Button targetButton = menuButtons[_currentIndex];
        if (targetButton == null) return;

        RectTransform targetBtnRect = targetButton.GetComponent<RectTransform>();

        // 선택 버튼의 위치 = (타겟 버튼의 중심 + 타겟 버튼의 너비 / 2 + 오프셋)
        Vector3 finalPos = targetBtnRect.position;
        float halfWidth = (targetBtnRect.rect.width / 2f) * targetBtnRect.lossyScale.x;
        finalPos.x += halfWidth;
        cursor.position = finalPos + (Vector3)cursorOffset;
    }

}

