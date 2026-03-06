using System.Collections;
using UnityEngine;

public class UI_EventListener : MonoBehaviour
{
    private bool _ignoreInput = false;

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

    private IEnumerator IgnoreInputForMoment()
    {
        _ignoreInput = true;
        yield return null;
        _ignoreInput = false;
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
            StartCoroutine(IgnoreInputForMoment()); // 한 프레임동안 남아있는 입력 방지
        }
    }

    private void HandleUIInput(Vector2 dir)
    {
        if (_ignoreInput) return;

        var popup = Managers.UI.GetTopPopup();
        if (popup != null)
        {
            popup.OnInput(dir);
        }
    }

    private void HandleUISubmit()
    {
        if (_ignoreInput) return;

        var popup = Managers.UI.GetTopPopup();
        if (popup != null)
        {
            popup.OnSubmit();
        }
    }
    private void HandleUICancel()
    {
        if (_ignoreInput) return;

        var popup = Managers.UI.GetTopPopup();
        if (popup != null)
        {
            popup.OnCancel();
        }

        if (Managers.UI.PopupCount == 0)
        {
            Managers.Input.SetInputMode(false);
            Time.timeScale = 1f;
        }
    }
}
