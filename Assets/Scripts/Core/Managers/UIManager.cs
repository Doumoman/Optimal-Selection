using System.Collections.Generic;
using UnityEngine;

public class UIManager : IManager
{
    private bool _init = false;
    private int _order = 10;
    private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    private UI_Scene _sceneUI = null;
    public int PopupCount => _popupStack.Count;
    private GameObject _root;
    public GameObject Root
    {
        get
        {
            if (_root == null)
            {
                _root = GameObject.Find("@UI_Root");
                if (_root == null)
                {
                    _root = new GameObject { name = "@UI_Root" };
                }
            }
            return _root;
        }
    }

    public void Init()
    {
        if (_init) return;
        _init = true;

        // 초기화 로직
        _order = 10;
    }

    public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvas.overrideSorting = true;
            if (sort)
            {
                canvas.sortingOrder = _order;
                _order++;
            }
            else
            {
                canvas.sortingOrder = sortOrder;
            }
        }
        else
        {
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }

            if (sort) // WorldSpave UI는 Sorting Layer 설정이 중요한 경우가 많음
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortOrder;
            }
        }
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        if (_sceneUI != null)
        {
            Managers.Resource.Destroy(_sceneUI.gameObject);
            _sceneUI = null;
        }

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");

        if (go == null)
        {
            Debug.LogError($"[UIManager] Failed to load Scene UI: {name}");
            return null;
        }

        go.transform.SetParent(Root.transform);

        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");

        if (go == null)
        {
            Debug.LogError($"[UIManager] Failed to load Popup UI: {name}");
            return null;
        }

        go.transform.SetParent(Root.transform);

        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        return popup;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0) return;

        // 스택의 맨 위에 있는 것과 닫으려는 것이 다르면 에러 (순서 꼬임 방지)
        if (_popupStack.Peek() != popup)
        {
            Debug.LogError("Close Popup Failed! Top popup is not the requested popup.");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0) return;

        UI_Popup popup = _popupStack.Pop();

        Managers.Resource.Destroy(popup.gameObject);

        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _sceneUI = null;
        _root = null;
        _order = 10;
    }

    public void OnDestroy()
    {
        CloseAllPopupUI();
        _popupStack = null;
        _sceneUI = null;
    }
}