using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChapterState : ISerializationCallbackReceiver
{
    public int chapterID;
    public bool isUnlocked;
    public bool isCleared;

    [NonSerialized]
    public Dictionary<int, MapState> mapStates = new Dictionary<int, MapState>(); // key: ID, value: ID를 갖고 있는 맵 상태

    [SerializeField]
    private List<MapState> _savedMapList = new List<MapState>(); // 딕셔너리는 JsonUtility가 무시하므로, 세이브 용 리스트 생성

    public ChapterState(int id)
    {
        chapterID = id;
    }

    // 저장하기 직전에 자동 호출 ( Dictionary -> List )
    public void OnBeforeSerialize()
    {
        _savedMapList.Clear();
        foreach(var pair in mapStates)
        {
            _savedMapList.Add(pair.Value);
        }
    }

    // 저장한 후에 자동 호출 ( List -> Dictionary )
    public void OnAfterDeserialize()
    {
        mapStates.Clear();
        foreach (var map in _savedMapList)
        {
            if (!mapStates.ContainsKey(map.mapID))
            {
                mapStates.Add(map.mapID, map);
            }
        }
    }
}
