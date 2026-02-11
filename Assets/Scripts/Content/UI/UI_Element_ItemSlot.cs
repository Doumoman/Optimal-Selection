using UnityEngine;
using UnityEngine.UI;

public class UI_Element_ItemSlot : MonoBehaviour
{
    private ItemData _itemData;
    public void OnSlotClicked()
    {
        // 아이템 사용 로직...
        // 아마 ItemManager의 함수 호출
        Debug.Log("아이템 사용됨!");
    }

    public void SetItem(ItemData data)
    {
        // 아이템데이터를 받아와서 세팅
        _itemData = data;
    }
}
