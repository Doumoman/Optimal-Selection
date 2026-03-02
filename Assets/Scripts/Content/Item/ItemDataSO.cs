using UnityEngine;
[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public class ItemDataSO : ScriptableObject
{
    [Header("Base Info")]
    public int itemID;          // 아이템 ID
    public string itemStringId; // 아이템 문자열 ID
    public string itemName;     // 이름
    public Sprite icon;         // 아이콘 이미지

    [TextArea]
    public string description;  // 아이템 설명

    public int maxStack = 99;   // 한 슬롯에 최대 몇 개까지?
    public ItemType type;       // 아이템 타입 (소모품, 장비 ...)
}
