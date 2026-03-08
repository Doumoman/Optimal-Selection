using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : IManager
{
    private bool _init = false;

    // 대화 데이터를 캐싱하는 Dictionary
    private Dictionary<string, DialogueData> _dialogueDB = new Dictionary<string, DialogueData>();

    // 현재 진행 중인 대화 데이터와 열려있는 팝업 UI 참조
    private DialogueData _currentDialogue;
    private UI_Popup_Dialogue _currentPopup;
    private Action<string> _onDialogueEndCallBack;
    private string _initialStartID;

    public event Action<string> OnDialogueEvent; // 이벤트의 이름을 넘겨준다
    public event Action OnDialogueEnd;
    public Func<string, string> OnBranchDecide; // 델리게이트

    [Serializable]
    public class DialogueData
    {
        public string ID;
        public string Speaker;
        public string DialogueText;
        public string PortraitName;
        public string NextID;
        public string NextStartID;
        public string EventName;

        // 다회차, 엔딩 분기를 위한 데이터
        public string FailID; // RequiredEnding 조건을 만족하지 못할시의 다음 대화 ID
        public string RequiredEnding; // 특정 엔딩을 봐야만 볼 수 있는 대화에 이 값을 채운다. ex) Ending_Normal -> 노말 엔딩을 본 적 있어야 볼 수 있음
    }

    public void Init()
    {
        if (_init) return;
        _init = true;
    }

    public void LoadDialogueData(string csvFileName)
    {
        _dialogueDB.Clear();
        List<Dictionary<string, object>> parsedData = CSVParser.Read(csvFileName);

        if (parsedData == null || parsedData.Count == 0)
        {
            Debug.LogError($"[DialogueManager] CSV 파싱 실패 또는 데이터가 없습니다: {csvFileName}");
            return;
        }

        for (int i = 0; i < parsedData.Count; i++)
        {
            var row = parsedData[i];
            DialogueData data = new DialogueData();

            data.ID = row.ContainsKey("ID") ? row["ID"].ToString() : "";
            data.Speaker = row.ContainsKey("Speaker") ? row["Speaker"].ToString() : "";
            data.DialogueText = row.ContainsKey("Dialogue") ? row["Dialogue"].ToString() : "";
            data.PortraitName = row.ContainsKey("Portrait") ? row["Portrait"].ToString() : "";
            data.NextID = row.ContainsKey("NextID") ? row["NextID"].ToString() : "";
            data.NextStartID = row.ContainsKey("NextStartID") ? row["NextStartID"].ToString() : "";
            data.EventName = row.ContainsKey("EventName") ? row["EventName"].ToString() : "";

            data.RequiredEnding = row.ContainsKey("RequiredEnding") ? row["RequiredEnding"].ToString() : "";
            data.FailID = row.ContainsKey("FailID") ? row["FailID"].ToString() : "";

            if (string.IsNullOrEmpty(data.NextID)) // 분기점이 아닌 대화
            {
                // 다음 줄이 존재한다면
                if (i + 1 < parsedData.Count)
                {
                    var nextRow = parsedData[i + 1];
                    string nextRowId = nextRow.ContainsKey("ID") ? nextRow["ID"].ToString() : "";

                    if (!string.IsNullOrEmpty(nextRowId))
                    {
                        data.NextID = nextRowId; // 분기점 존재
                    }
                    else
                    {
                        data.NextID = "End";
                    }
                }
                else
                {
                    // CSV의 마지막 줄
                    data.NextID = "End";
                }
            }

            if (!string.IsNullOrEmpty(data.ID))
            {
                _dialogueDB[data.ID] = data;
            }
        }

        Debug.Log($"[DialogueManager] 대화 데이터 로드 완료: 총 {_dialogueDB.Count}건");
    }

    /// <summary>
    /// 엔딩 조건을 검사하여 실제로 화면에 출력할 유효한 대화 데이터를 찾아 반환
    /// </summary>
    private DialogueData GetValidDialogue(string startID)
    {
        string currentID = startID;

        // 조건에 맞는 대화를 찾을 때까지 FailID를 타고 계속 검색
        while (!string.IsNullOrEmpty(currentID) && _dialogueDB.ContainsKey(currentID))
        {
            DialogueData data = _dialogueDB[currentID];

            // RequiredEnding 칸이 비어있거나, 해당 엔딩을 클리어한 기록이 있다면 통과
            if (string.IsNullOrEmpty(data.RequiredEnding) || CheckIfEndingCleared(data.RequiredEnding))
            {
                return data;
            }
            else
            {
                // 불합격이면 FailID로 점프하여 대체 대화를 다시 검사
                currentID = data.FailID;
            }
        }

        return null; // 끝까지 유효한 대화를 찾지 못함
    }

    private bool CheckIfEndingCleared(string endingName)
    {
        // TODO: 실제 게임에서는 SaveManager 등에서 저장된 엔딩 목록을 확인해야함
        return false;
    }

    public void StartDialogue(string startID, Action<string> onDialogueEnd = null)
    {
        _onDialogueEndCallBack = onDialogueEnd;
        _initialStartID = startID;

        string targetID = startID;

        if (OnBranchDecide != null) // 분기가 있을 때
        {
            string branchedID = OnBranchDecide.Invoke(startID);
            if (!string.IsNullOrEmpty(branchedID))
            {
                targetID = branchedID; // 대화 시작용 더미 아이디를 실제 CSV 파일에 존재하는 아이디로 변경
            }
        }

        _currentDialogue = GetValidDialogue(targetID);

        if (_currentDialogue == null)
        {
            Debug.LogWarning($"[DialogueManager] 유효한 대화 ID를 찾을 수 없습니다. 요청ID: {startID}, 변환된ID: {targetID}");
            return;
        }

        if (_currentPopup == null)
        {
            _currentPopup = Managers.UI.ShowPopupUI<UI_Popup_Dialogue>("UI_Popup_Dialogue");
        }

        _currentPopup.SetDialogue(_currentDialogue);
    }

    public void ProcessNextDialogue()
    {
        if (_currentDialogue == null) return;

        string nextID = _currentDialogue.NextID;

        if (OnBranchDecide != null) // 동적 분기
        {
            string branchedID = OnBranchDecide.Invoke(_currentDialogue.ID); // 현재 ID를 델리게이트로 넘겨줌, 처리 후 분기점 ID를 반환받는다!
            if (!string.IsNullOrEmpty(branchedID))
            {
                nextID = branchedID;
            }
        }

        // 다음 대화 진행 또는 대화 종료
        if (string.IsNullOrEmpty(nextID) || nextID == "End")
        {
            EndDialogue();
        }
        else
        {
            _currentDialogue = GetValidDialogue(nextID);

            if (_currentDialogue != null)
            {
                _currentPopup.SetDialogue(_currentDialogue);
            }
            else
            {
                Debug.LogWarning($"[DialogueManager] 유효한 다음 대화를 찾을 수 없어 대화를 종료합니다: {nextID}");
                EndDialogue();
            }
        }
    }

    public void TriggerDialogueEvent(string eventNames)
    {
        if (string.IsNullOrEmpty(eventNames)) return;

        // 쉼표(,)를 기준으로 문자열을 쪼깸
        string[] events = eventNames.Split(',');

        foreach (string evt in events) // 여러 이벤트 처리 가능
        {
            // 앞뒤 공백을 제거
            string cleanEvent = evt.Trim();

            if (!string.IsNullOrEmpty(cleanEvent))
            {
                OnDialogueEvent?.Invoke(cleanEvent);
            }
        }
    }

    public void EndDialogue()
    {
        if (_currentPopup != null)
        {
            Managers.UI.ClosePopupUI(_currentPopup);
            _currentPopup = null;
        }

        string idToSave = _initialStartID;

        if(_currentDialogue != null && !String.IsNullOrEmpty(_currentDialogue.NextStartID))
        {
            idToSave = _currentDialogue.NextStartID;
        }

        _onDialogueEndCallBack?.Invoke(idToSave);

        _currentDialogue = null;
        OnDialogueEnd?.Invoke();
        ClearEvents();
    }

    public void ClearEvents()
    {
        OnDialogueEvent = null;
        OnDialogueEnd = null;

        // OnBranchDecide는 일회성 이벤트가 아님
    }

    public void Clear()
    {
        _dialogueDB.Clear();
        _currentPopup = null;
        ClearEvents();
    }

    public void OnDestroy()
    {
        Clear();
        _init = false;
    }
}