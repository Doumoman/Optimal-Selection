using UnityEngine;

public class UI_EventListener : MonoBehaviour
{
    private void Start()
    {
        // UI 관련 입력 이벤트들 구독
        Managers.Input.OnMenuPressed -= HandleEscKey;
        Managers.Input.OnMenuPressed += HandleEscKey;
    }

    private void SetPlayerInput(bool allowInput)
    {
        // 현재 씬의 플레이어를 찾아서 설정
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.isInputBlocked = !allowInput;
        }
    }

    private void HandleEscKey()
    {
        int currentPopupCount = Managers.UI.PopupCount;
        if (currentPopupCount > 0)
        {
            Managers.UI.ClosePopupUI();

            if (Managers.UI.PopupCount == 0)
            {
                SetPlayerInput(true);
            }
        }
        else
        {
            Managers.UI.ShowPopupUI<UI_Popup_Menu>("UI_Popup_Menu");
            SetPlayerInput(false);
        }
    }
}
