using UnityEngine;

public class UI_EventListener : MonoBehaviour
{
    private void Start()
    {
        // UI 관련 입력 이벤트들 구독
        Managers.Input.OnMenuPressed -= HandleGamePlayMenu;
        Managers.Input.OnMenuPressed += HandleGamePlayMenu;

        Managers.Input.OnInput -= HandleUIInput;
        Managers.Input.OnInput += HandleUIInput;

        Managers.Input.OnSubmitPressed -= HandleUISubmit;
        Managers.Input.OnSubmitPressed += HandleUISubmit;

        Managers.Input.OnCancelPressed -= HandleUICancel;
        Managers.Input.OnCancelPressed += HandleUICancel;
    }

    private void HandleGamePlayMenu()
    {
        int currentPopupCount = Managers.UI.PopupCount;
        if (currentPopupCount > 0)
        {
            Managers.UI.ClosePopupUI();

            if (Managers.UI.PopupCount == 0)
            {
                Managers.Input.SetInputMode(false);
            }
        }
        else
        {
            Managers.UI.ShowPopupUI<UI_Popup_Menu>("UI_Popup_Menu");
            Managers.Input.SetInputMode(true);
        }
    }

    private void HandleUIInput(Vector2 dir)
    {
        var popup = Managers.UI.GetTopPopup();
        if (popup != null)
        {
            popup.OnInput(dir);
        }
    }

    private void HandleUISubmit()
    {
        var popup = Managers.UI.GetTopPopup();
        if (popup != null)
        {
            popup.OnSubmit();
        }
    }
    private void HandleUICancel()
    {
        var popup = Managers.UI.GetTopPopup();
        if (popup != null)
        {
            popup.OnCancel();
        }

        if (Managers.UI.PopupCount == 0)
        {
            Managers.Input.SetInputMode(false);
        }
    }
}
