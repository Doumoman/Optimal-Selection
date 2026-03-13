using System.Collections.Generic;

[System.Serializable]
public class MapState
{
    public int mapID;

    public List<int> deadMonsterUniqueIds = new List<int>();  // 죽은 몬스터의 배치 ID
    public List<int> lootedItemUniqueIds = new List<int>();   // 획득한 아이템의 배치 ID
    public List<int> interactedObjectUniqueIds = new List<int>(); // 상호작용한 오브젝트 ID

    public MapState(int id)
    {
        this.mapID = id;
    }
}
