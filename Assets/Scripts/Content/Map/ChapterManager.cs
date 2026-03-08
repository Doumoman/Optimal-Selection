using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChapterManager : MonoBehaviour
{
    [Header("Chapter Settings")]
    public string chapterCSVFileName;

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

}
