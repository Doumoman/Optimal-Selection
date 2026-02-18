using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Popup_Inventory : UI_Popup
{
    [SerializeField] private Transform _gridParent;
    [SerializeField] private RectTransform _cursor;
    [SerializeField] private TextMeshProUGUI _pageText;

    private int _selectedIndex = 0;
    private int _columnCount = 3;
    private int _pageSize = 6;
    private int _pageCount = 2;

    private List<UI_Element_ItemSlot> _itemSlots = new List<UI_Element_ItemSlot>();
    private List<ItemData> _allItems = new List<ItemData>(); // 실제 보유한 아이템은 나중에 ItemManager로 관리하자

    private Vector2 offset = new Vector2(10f, 0);
    private bool _init;
    private bool _canInput;

    public override void Init()
    {
        if (_init) return;
        base.Init();
        _init = true;

        // 테스트 용 아이템 12개
        _allItems.Clear();
        for (int i = 0; i < 12; i++)
        {
            _allItems.Add(new ItemData { name = $"아이템 {i}", description = $"설명 {i}" });
        }

        foreach (Transform child in _gridParent)
            Managers.Resource.Destroy(child.gameObject);
        _itemSlots.Clear();

        for (int i = 0; i < _pageSize; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Element/UI_Element_ItemSlot", _gridParent);
            _itemSlots.Add(go.GetComponent<UI_Element_ItemSlot>());
        }

        _selectedIndex = 0;
        _columnCount = 3;
        _pageSize = 6;

        UpdatePage();
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
        int currentPage = _selectedIndex / _pageSize;
        int lastPage = (_allItems.Count - 1) / _pageSize;

        // 1. 방향에 따른 인덱스 계산
        if (direction.x > 0) // 오른쪽
        {
            // 현재 열이 마지막 열인지 확인
            if (_selectedIndex % _columnCount == _columnCount - 1)
            {
                if (currentPage < lastPage)
                {
                    // 마지막 페이지가 아닌 경우에만 다음 페이지의 같은 행 첫 번째 열로 이동
                    _selectedIndex += (_pageSize - _columnCount + 1);
                }
                else
                {
                    // 마지막 페이지의 마지막 열인 경우 단순 플러스
                    _selectedIndex++;
                }
            }
            else
            {
                _selectedIndex++;
            }
        }
        else if (direction.x < 0) // 왼쪽
        {
            // 현재 열이 첫 번째 열인지 확인
            if (_selectedIndex % _columnCount == 0)
            {
                if (currentPage > 0)
                {
                    // 첫 페이지가 아닐 경우에만 이전 페이지의 같은 행 마지막 열로 이동
                    _selectedIndex -= (_pageSize - _columnCount + 1);
                }
                else
                {
                    // 첫 번째 페이지의 첫 번째 열인 경우 단순 마이너스
                    _selectedIndex--;
                }
            }
            else
            {
                _selectedIndex--;
            }
        }
        else if (direction.y < 0) // 아래 (행 변경)
        {
            _selectedIndex += _columnCount;
        }
        else if (direction.y > 0) // 위 (행 변경)
        {
            _selectedIndex -= _columnCount;
        }

        // 2. 범위 제한 (0 ~ 마지막 아이템)
        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _allItems.Count - 1);

        // 3. 인덱스가 바뀌었을 때만 UI 갱신
        if (prevIndex != _selectedIndex)
        {
            int prevPage = prevIndex / _pageSize;
            int curPage = _selectedIndex / _pageSize;

            if(prevPage != curPage)
            {
                UpdatePage();
            }

            UpdateCursor();
        }
    }

    public override void OnSubmit() // 선택/결정 로직(Enter)
    {
        if (!_canInput) return;
        if (_itemSlots.Count == 0) return;

        Debug.Log($"선택된 아이템 인덱스: {_selectedIndex}");
        _itemSlots[_selectedIndex % _pageSize].OnSlotClicked(); // 아이템 설명, 사용 팝업 띄우기
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
        if (_itemSlots == null || _itemSlots.Count <= _selectedIndex % _pageSize || _cursor == null) return;

        RectTransform targetRect = _itemSlots[_selectedIndex % _pageSize].GetComponent<RectTransform>();

        // 선택 버튼의 위치 = (타겟 버튼의 중심 + 타겟 버튼의 너비 / 2 + 오프셋)
        Vector3 finalPos = targetRect.position;
        float halfWidth = (targetRect.rect.width / 2f) * targetRect.lossyScale.x;
        finalPos.x += halfWidth;
        _cursor.position = finalPos + (Vector3)offset;

        // _selectedIndex = 2,5에서 오른쪽 방향키 누르면 다음 페이지로 넘어가야함
        // _selectedIndex = 6,9에서 왼쪽 방향키 누르면 이전 페이지로 넘어가야함
    }

    private void UpdatePage()
    {
        int currentPage = _selectedIndex / _pageSize; // 0 or 1
        int startDataIndex = currentPage * _pageSize; // 0 or 6

        _pageText.text = $"페이지 {currentPage + 1} / {_pageCount}";

        for (int i = 0; i < _pageSize; i++)
        {
            int dataIndex = startDataIndex + i; // 보여줄 데이터 번호

            if (dataIndex < _allItems.Count)
            {
                // 데이터가 있으면 그 슬롯에 아이템 세팅
                _itemSlots[i].SetItem(_allItems[dataIndex]);
            }
            else
            {
                // 데이터가 없으면 빈 슬롯 처리
                _itemSlots[i].SetItem(null);
            }
        }
    }
}
