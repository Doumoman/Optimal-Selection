using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Popup_Inventory : UI_Popup
{
    [SerializeField] private Transform _gridParent;
    [SerializeField] private RectTransform _cursor;

    private int _selectedIndex = 0;
    private int _columnCount = 3;
    public List<UI_Element_ItemSlot> _itemSlots = new List<UI_Element_ItemSlot>();

    private Vector2 offset = new Vector2(10f, 0);
    private bool _init;
    private bool _canInput;

    public override void Init()
    {
        if (_init) return;
        base.Init();
        _init = true;

        _selectedIndex = 0;
        _columnCount = 3;

        _canInput = false;
        StartCoroutine(CoInitCursorPosition());
    }

    private void Start()
    {
        Init();
    }

    public override void OnInput(Vector2 direction) // 방향키 이동 로직
    {
        if (!_canInput) return;
        if (_itemSlots.Count == 0) return;

        int prevIndex = _selectedIndex;

        // 1. 방향에 따른 인덱스 계산
        if (direction.x > 0) _selectedIndex++;             // 오른쪽
        else if (direction.x < 0) _selectedIndex--;        // 왼쪽
        else if (direction.y < 0) _selectedIndex += _columnCount; // 아래 (행 변경)
        else if (direction.y > 0) _selectedIndex -= _columnCount; // 위 (행 변경)

        // 2. 범위 제한 (0 ~ 마지막 아이템)
        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _itemSlots.Count - 1);

        // 3. 인덱스가 바뀌었을 때만 UI 갱신
        if (prevIndex != _selectedIndex)
        {
            UpdateCursor();
        }
    }

    public override void OnSubmit() // 선택/결정 로직(Enter)
    {
        if (!_canInput) return;
        if (_itemSlots.Count == 0) return;

        Debug.Log($"선택된 아이템 인덱스: {_selectedIndex}");
        _itemSlots[_selectedIndex].OnSlotClicked(); // 아이템 설명, 사용 팝업 띄우기
    }

    public override void OnCancel() // 취소/닫기(esc) 로직
    {
        if (!_canInput) return;
        base.OnCancel();
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
        if (_itemSlots == null || _itemSlots.Count <= _selectedIndex || _cursor == null) return;

        RectTransform targetRect = _itemSlots[_selectedIndex].GetComponent<RectTransform>();

        // 선택 버튼의 위치 = (타겟 버튼의 중심 + 타겟 버튼의 너비 / 2 + 오프셋)
        Vector3 finalPos = targetRect.position;
        float halfWidth = (targetRect.rect.width / 2f) * targetRect.lossyScale.x;
        finalPos.x += halfWidth;
        _cursor.position = finalPos + (Vector3)offset;

        // _selectedIndex = 5에서 오른쪽 방향키 누르면 다음 페이지로 넘어가야함
        // _selectedIndex = 6에서 왼쪽 방향키 누르면 이전 페이지로 넘어가야함
    }
}
