using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    #region 플레이어 저장 요소

    public int currentMapId = 1; // 맵 번호
    public string currentMapStringId = string.Empty; // 맵 고유 ID

    public float playerHealth = 100f; // 체력
    public int playerMalice = 0; // 악의
    public int playerGold = 0; // 골드

    public float playTime = 0f; // 플레이 시간

    // 인벤토리
    public List<InventoryItemData> playerInventory = new List<InventoryItemData>();

    #endregion

    #region 이벤트 저장 요소

    public int monstersKilled = 0; // 죽인 몬스터의 수

    // 클리어 한 챕터 번호를 저장
    public List<int> clearedChapters = new List<int>();

    // 처치된 핵심 인물의 고유 ID 저장
    public List<string> killedMainNPCIds = new List<string>();

    #endregion

    #region 월드 상태 & 상호작용 저장 요소

    // 상호작용 가능 오브젝트의 고유 ID 목록, 이 리스트에 없으면 상호작용 불가능
    public List<string> interactedObjectIds = new List<string>();

    // 회수/사용 아이템의 고유 ID 목록, 이 리스트에 없으면 드랍되어 있는 아이템
    public List<string> lootedItemIds = new List<string>();

    // 해금된 장소의 고유 ID 목록, 이 리스트에 없으면 잠겨있는 장소
    public List<string> unlockedMapIds = new List<string>();

    #endregion
}

[Serializable]
public class InventoryItemData
{
    public string itemId;
    public int amount;
    // 아이템 종류?
    // 사용할 수 있는 아이템인지?
    // ....

    public InventoryItemData(string id, int amount)
    {
        this.itemId = id;
        this.amount = amount;   
    }
}