using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChapterManager : MonoBehaviour
{
    [Header("Chapter Settings")]
    public string chapterCSVFileName;

    [Header("Current Chapter Info")]
    public ChapterDataSO currentChapterData;
    public ChapterState currentChapterState;
    public Vector2Int CurrentCoords {  get; private set; }

    protected Dictionary<string, Func<string>> _branchRouters = new Dictionary<string, Func<string>>();

    private void Start()
    {
        InitChapterDialogue();
        InitChapterMap();
        RegisterChapterBranches();

        Managers.Dialogue.OnBranchDecide += HandleChapterBranch;
    }

    protected abstract void RegisterChapterBranches();

    public void InitChapterMap()
    {
        if(currentChapterData == null)
        {
            Debug.LogError("[ChapterManager] 챕터 데이터(ChapterDataSO)가 할당되지 않았습니다!");
            return;
        }
        // 챕터 시작 맵 좌표 설정
        CurrentCoords = currentChapterData.startMapCoords;

        currentChapterState = Managers.Data.GetOrCreateChapterState(currentChapterData.chapterID);
        if (currentChapterState == null)
        {
            Debug.Log("[ChapterManager] 세이브된 챕터 상태가 없습니다. 새로 생성합니다.");
            currentChapterState = new ChapterState(currentChapterData.chapterID);
        }
        
        // 시작 맵 정적 데이터 가져오기
        MapDataSO startMapDataSO = GetStartMapData();
        if(startMapDataSO == null)
        {
            Debug.LogError($"[ChapterManager] 좌표 {CurrentCoords} 에 해당하는 시작 맵 데이터가 없습니다!");
            return;
        }

        // 시작 맵 동적 데이터 가져오기
        MapState startMapState = currentChapterState.GetOrCreateMapState(currentChapterData.chapterID, startMapDataSO.mapID);

        // 맵 이동 요청
        Managers.Map.TransitionMap(startMapState, startMapDataSO);
    }

    public void InitChapterDialogue()
    {
        if (string.IsNullOrEmpty(chapterCSVFileName))
        {
            Debug.Log("불러올 챕터 대화 CSV 파일 이름이 비어있습니다!");
        }

        Managers.Dialogue.LoadDialogueData(chapterCSVFileName);
    }

    public string HandleChapterBranch(string currentID)
    {
        if (_branchRouters.TryGetValue(currentID, out Func<string> checkFunc))
        {
            return checkFunc.Invoke();
        }
        return null;
    }

    /// <summary>
    /// 맵 좌표에 맞는 맵 데이터를 가져옴
    /// </summary>
    public MapDataSO GetMapData(Vector2Int coords)
    {
        if (currentChapterData == null) return null;

        foreach (var node in currentChapterData.chapterMapNodes)
        {
            if (node.coordinates == coords) return node.mapData;
        }
        return null;
    }

    /// <summary>
    /// 시작 맵 데이터 바로 가져오기
    /// </summary>
    public MapDataSO GetStartMapData()
    {
        if (currentChapterData == null) return null;
        return GetMapData(currentChapterData.startMapCoords);
    }
}
