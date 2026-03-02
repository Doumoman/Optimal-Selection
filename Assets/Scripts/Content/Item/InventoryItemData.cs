// 인벤토리, 세이브 데이터에 저장될 데이터
[System.Serializable]
public class InventoryItemData
{
    public int itemId;
    public string itemStringId;
    public int count;

    public InventoryItemData(int id, string stringId, int count)
    {
        this.itemId = id;
        this.itemStringId = stringId;
        this.count = count;
    }
}