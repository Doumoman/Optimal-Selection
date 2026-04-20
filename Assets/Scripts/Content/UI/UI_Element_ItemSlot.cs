using UnityEngine;
using UnityEngine.UI;

public class UI_Element_ItemSlot : MonoBehaviour
{
    private ItemDataSO _itemData;

    public void OnSlotClicked()
    {
        SingletonManagers.UI.ShowPopupUI<UI_Popup_ItemInfo>("UI_Popup_ItemInfo");
    }

    public void SetItem(ItemDataSO data)
    {
        // 아이템 데이터를 받아와서 세팅
        _itemData = data;
    }
}
