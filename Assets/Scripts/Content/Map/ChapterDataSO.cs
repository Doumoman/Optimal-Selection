using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 챕터의 초기(정적) 정보를 가지고 있음
/// </summary>
[CreateAssetMenu(fileName = "ChapterDataSO", menuName = "Scriptable Objects/ChapterDataSO")]
public class ChapterDataSO : ScriptableObject
{
    [Header("Base Info")]
    public int chapterID;              // 고유 ID
    public string chapterStringID;     // 고유 문자열 ID
    public string chapterName;         // 챕터 이름

    [Header("Scene Settings")]
    public string sceneName;           // 로드할 씬 이름
    public Vector2Int startMapCoords;  // 챕터 진입 시 시작할 맵 좌표 (0,0)

    [Header("Map Structure")]
    // 이 챕터에 포함된 모든 맵 리스트 (좌표 <-> 맵 데이터)
    public List<ChapterMapNode> mapNodes = new List<ChapterMapNode>();

    // 좌표로 맵 데이터 찾기
    public MapDataSO GetMapData(Vector2Int coords)
    {
        foreach (var node in mapNodes)
        {
            if (node.coordinates == coords) return node.mapData;
        }
        return null;
    }

    // 시작 맵 데이터 바로 가져오기
    public MapDataSO GetStartMapData()
    {
        return GetMapData(startMapCoords);
    }
}

// 각 맵의 주소를 좌표로 표현
[System.Serializable]
public class ChapterMapNode
{
    public Vector2Int coordinates;
    public MapDataSO mapData;
}
