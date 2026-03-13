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

    protected Dictionary<string, Func<string>> _branchRouters = new Dictionary<string, Func<string>>();

    private void Start()
    {
        InitChapterDialogue();
        RegisterChapterBranches();

        Managers.Dialogue.OnBranchDecide += HandleChapterBranch;
    }

    protected abstract void RegisterChapterBranches();

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

    public MapDataSO GetMapData(Vector2Int coords)
    {
        if (currentChapterData == null) return null;

        foreach (var node in currentChapterData.mapNodes)
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
