using System.Collections.Generic;

[System.Serializable]
public class ChapterState
{
    public int chapterID;
    public bool isUnlocked;
    public bool isCleared;

    // 챕터 내의 방(Map)별 상태들
    // Key: 맵 ID (또는 좌표 해시값), Value: 방 상태
    public Dictionary<int, MapState> mapStates = new Dictionary<int, MapState>();

    public ChapterState(int id)
    {
        chapterID = id;
        isUnlocked = false;
        isCleared = false;
    }
}
