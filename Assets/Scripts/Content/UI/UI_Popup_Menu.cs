using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
UI_Popup_Menu : Esc를 눌렀을 때 캐릭터의 현재 상태, 목표, 인벤토리를 고를 수 있는 창
UI_Popup_Inventory : 메뉴에서 인벤토리를 선택했을때 뜨는 팝업, 여기서 아이템 슬롯을 고를 수 있다
UI_Element_ItemSlot : 인벤토리에서 각각의 아이템 슬롯을 담당하는 프리팹
UI_Popup_ItemInfo : 인벤토리에서 하나의 아이템 슬롯을 선택했을 때 뜨는 팝업, 그 아이템의 정보를 띄우고 사용할 것인지 버릴 것인지 등을 결정한다.
 */

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
    [SerializeField] private RectTransform _cursor;
    [SerializeField] private Vector2 _cursorOffset;

    private int _currentIndex = 0;
    private bool _init = false;
    private bool _canInput = false;
    public override void Init()
    {
        if (_init) return;
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
        if (menuButtons == null || menuButtons.Length <= _currentIndex || _cursor == null)
            return;

        Button targetButton = menuButtons[_currentIndex];
        if (targetButton == null) return;

        RectTransform targetBtnRect = targetButton.GetComponent<RectTransform>();

        // 실제 월드 좌표 기준으로 계산
        // corners[0]: 좌측 하단 / corners[1]: 좌측 상단 / corners[2]: 우측 상단 / corners[3]: 우측 하단
        Vector3[] corners = new Vector3[4];
        targetBtnRect.GetWorldCorners(corners);

        Vector3 rightCenterPos = (corners[2] + corners[3]) / 2f;
        _cursor.position = rightCenterPos + (Vector3)_cursorOffset;
    }
}

