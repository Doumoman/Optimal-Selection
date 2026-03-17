using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChapterState : ISerializationCallbackReceiver
{
    public int chapterID;
    public bool isUnlocked = false;
    public bool isCleared = false;

    [NonSerialized]
    public Dictionary<int, MapState> chapterMapStates = new Dictionary<int, MapState>(); // 챕터에 존재하는 각 맵의 동적 상태

    [SerializeField]
    private List<MapState> _savedMapList = new List<MapState>(); // 딕셔너리는 JsonUtility가 무시하므로, 세이브 용 리스트 생성

    public ChapterState(int id)
    {
        chapterID = id;
        isUnlocked = true;
    }

    // 저장하기 직전에 자동 호출 ( Dictionary -> List )
    public void OnBeforeSerialize()
    {
        if (chapterMapStates == null) chapterMapStates = new Dictionary<int, MapState>();
        if(_savedMapList == null) _savedMapList = new List<MapState>();

        _savedMapList.Clear();

        foreach(var pair in chapterMapStates)
        {
            _savedMapList.Add(pair.Value);
        }
    }

    // 저장한 후에 자동 호출 ( List -> Dictionary )
    public void OnAfterDeserialize()
    {
        if (chapterMapStates == null) chapterMapStates = new Dictionary<int, MapState>();
        if (_savedMapList == null) _savedMapList = new List<MapState>();

        chapterMapStates.Clear();

        foreach (var map in _savedMapList)
        {
            if (!chapterMapStates.ContainsKey(map.mapID))
            {
                chapterMapStates.Add(map.mapID, map);
            }
        }
    }

    /// <summary>
    /// 챕터 내에 있는 맵의 동적 상태를 가져옴
    /// </summary>
    public MapState GetOrCreateMapState(int mapId)
    {
        if(!chapterMapStates.TryGetValue(mapId, out MapState mapState))
        {
            mapState = new MapState(mapId);
            chapterMapStates.Add(mapId, mapState);
            _savedMapList.Add(mapState);
        }
        return mapState;
    }
}
