using UnityEngine;

public class Chapter0_Manager : ChapterManager
{
    [Header("Chapter 0 Specific")]
    [SerializeField] private int _wrongInteractionCount = 0;

    private readonly string stapiStartID = "Stapi_Tutorial_Start";

    protected override void RegisterChapterBranches()
    {
        // 분기 추가
        _branchRouters.Add(stapiStartID, CheckStapiInteraction);
    }

    public void AddWrongInteractionCount()
    {
        _wrongInteractionCount++;
        Debug.Log($"[Chapter0] 딴짓 횟수 증가! 현재: {_wrongInteractionCount}회");
    }

    private string CheckStapiInteraction()
    {
        string interactionCountOver10 = "Stapi_Tutorial_Branch03_01";
        string interactionCount7to9 = "Stapi_Tutorial_Branch02_01";
        string interactionCountUnder7 = "Stapi_Tutorial_Branch01_01";

        if (_wrongInteractionCount >= 10) return interactionCountOver10;
        else if (_wrongInteractionCount >= 7) return interactionCount7to9;
        else return interactionCountUnder7;
    }
}