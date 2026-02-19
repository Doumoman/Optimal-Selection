using UnityEngine;
using UnityEngine.UI;

public class UI_Element_ItemSlot : MonoBehaviour
{
    private ItemData _itemData;

    public void OnSlotClicked()
    {
        Managers.UI.ShowPopupUI<UI_Popup_ItemInfo>("UI_Popup_ItemInfo");
    }

    public void SetItem(ItemData data)
    {
        // 아이템데이터를 받아와서 세팅
        _itemData = data;
    }
}
