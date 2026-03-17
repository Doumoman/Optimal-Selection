using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    #region 플레이어 저장 요소

    public int currentChapterID = 0; // 현재 챕터 ID
    public string currenChapterStringID = "Chapter0"; // 현재 챕터 문자열 ID

    public int currentMapID = 1; // 맵 고유 ID
    public string currentMapStringID = "Chapter0_Observatory_5thFloor"; // 맵 고유 문자열 ID
    public Vector2 currentMapPosition = Vector2.zero; // 세이브 포인트 위치

    public float playTime = 0f; // 플레이 시간

    // 스탯
    public int playerHealth = 20; // 체력
    public int playerMalice = 0; // 악의
    public int playerLevel = 1; // 플레이어 레벨
    public int playerGold = 0; // 골드

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

    // 챕터의 동적 상태 리스트
    public List<ChapterState> chapterStates = new List<ChapterState>();
    #endregion
}