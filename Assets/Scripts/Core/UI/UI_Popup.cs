using UnityEngine;

public class UI_Popup : UI_Base
{
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, true);
    }

    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

    public virtual void OnInput(Vector2 direction) { } // 팝업 내 이동 입력 처리
    public virtual void OnSubmit() { } // 팝업 내에서 결정/확인  
    public virtual void OnCancel() { } // 팝업 닫기/취소
}
