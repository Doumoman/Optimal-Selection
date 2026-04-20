using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : IManager
{
    private bool _init = false;

    // 대화 데이터를 캐싱하는 Dictionary
    private Dictionary<string, DialogueData> _dialogueDB = new Dictionary<string, DialogueData>();

    // 대화 진행도를 저장하는 Dictionary (Key: NPC 더미 아이디, Value: 실제 출력할 대화 아이디)
    private Dictionary<string, string> _dialogueProgressDB = new Dictionary<string, string>();

    // 플레이어가 입력한 값들을 저장하는 Dictionary ( " PlayerName : 지나 ")
    // TODO: 게임 세이브 데이터에 저장 및 로드가 필요
    public Dictionary<string, string> StringVariables = new Dictionary<string, string>();

    // 현재 진행 중인 대화 데이터와 열려있는 팝업 UI 참조
    private DialogueData _currentDialogue;
    private UI_Popup_Dialogue _currentPopup;
    private UI_Popup_Select _currentSelectPopup;

    private Action<string> _onDialogueEndCallBack;
    private string _initialStartID;

    private bool _isWaitingForChoice = false;

    public event Action<string> OnDialogueEvent;
    public event Action OnDialogueEnd;
    public Func<string, string> OnBranchDecide;

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

        public string InputQuestion;
        public string InputKey;

        public string FailID;
        public string RequiredEnding;

        public List<string> Choices = new List<string>();
        public List<string> ChoiceNextIDs = new List<string>();
    }

    public void Init()
    {
        if (_init) return;
        _init = true;
    }

    // 게임 로드를 위한 함수
    public void LoadProgressData(Dictionary<string, string> savedProgress)
    {
        _dialogueProgressDB = savedProgress;
    }

    // 게임 세이브를 위한 함수
    public Dictionary<string, string> GetProgressData()
    {
        return _dialogueProgressDB;
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

            string rowID = row.ContainsKey("ID") ? row["ID"].ToString() : "";
            if (string.IsNullOrEmpty(rowID) || rowID.StartsWith("#") || rowID.StartsWith("=")) continue;

            DialogueData data = new DialogueData();

            data.ID = rowID;
            data.Speaker = row.ContainsKey("Speaker") ? row["Speaker"].ToString() : "";
            data.DialogueText = row.ContainsKey("Dialogue") ? row["Dialogue"].ToString() : "";
            data.PortraitName = row.ContainsKey("Portrait") ? row["Portrait"].ToString() : "";
            data.NextID = row.ContainsKey("NextID") ? row["NextID"].ToString() : "";
            data.NextStartID = row.ContainsKey("NextStartID") ? row["NextStartID"].ToString() : "";
            data.EventName = row.ContainsKey("EventName") ? row["EventName"].ToString() : "";

            data.InputQuestion = row.ContainsKey("InputQuestion") ? row["InputQuestion"].ToString() : "";
            data.InputKey = row.ContainsKey("InputKey") ? row["InputKey"].ToString() : "";

            data.RequiredEnding = row.ContainsKey("RequiredEnding") ? row["RequiredEnding"].ToString() : "";
            data.FailID = row.ContainsKey("FailID") ? row["FailID"].ToString() : "";

            for (int j = 1; j <= 4; j++) // 최대 4개의 선택지 (변동 가능)
            {
                string choiceCol = $"Choice{j}";
                string nextIDCol = $"NextID{j}";

                if (row.ContainsKey(choiceCol) && !string.IsNullOrEmpty(row[choiceCol].ToString()))
                {
                    data.Choices.Add(row[choiceCol].ToString());

                    string choiceNextID = row.ContainsKey(nextIDCol) ? row[nextIDCol].ToString() : "";
                    data.ChoiceNextIDs.Add(choiceNextID);
                }
            }

            if (string.IsNullOrEmpty(data.NextID) && data.Choices.Count == 0)
            {
                int nextIndex = i + 1;
                string nextRowId = "";

                while (nextIndex < parsedData.Count)
                {
                    var nextRow = parsedData[nextIndex];
                    string tempId = nextRow.ContainsKey("ID") ? nextRow["ID"].ToString() : "";

                    // 가독성을 위해 ID에 =가 붙일 수 있음
                    if (!string.IsNullOrEmpty(tempId) && !tempId.StartsWith("="))
                    {
                        nextRowId = tempId;
                        break;
                    }
                    nextIndex++;
                }

                if (!string.IsNullOrEmpty(nextRowId))
                {
                    data.NextID = nextRowId;
                }
                else
                {
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

    // 팝업에 대사를 넘겨주기 직전에, 중괄호 { } 로 묶인 글자를 유저가 입력한 값으로 바꿔치기
    // 원본 대사 대신 정제된 사본 대사를 넘겨준다
    public string ProcessTextVariables(string rawText)
    {
        string processedText = rawText;
        foreach (var variable in StringVariables)
        {
            processedText = processedText.Replace($"{{{variable.Key}}}", variable.Value);
        }
        return processedText;
    }

    /// <summary>
    /// 진행도 검사 -> 동적 분기 검사 -> 기획 조건 검사를 모두 거쳐 최종 대화 데이터를 반환
    /// </summary>
    private DialogueData GetValidDialogue(string requestID)
    {
        string currentID = requestID;

        // 1. 진행도(Progress) 검사
        if (_dialogueProgressDB.ContainsKey(currentID) && !string.IsNullOrEmpty(_dialogueProgressDB[currentID]))
        {
            currentID = _dialogueProgressDB[currentID];
        }

        // 2. 동적 코드 분기(Delegate) 검사
        if (OnBranchDecide != null)
        {
            string branchedID = OnBranchDecide.Invoke(currentID);
            if (!string.IsNullOrEmpty(branchedID))
            {
                currentID = branchedID;
            }
        }

        // 3. 다회차/엔딩 조건(Condition) 검사
        while (!string.IsNullOrEmpty(currentID) && _dialogueDB.ContainsKey(currentID))
        {
            DialogueData data = _dialogueDB[currentID];

            if (string.IsNullOrEmpty(data.RequiredEnding) || CheckIfEndingCleared(data.RequiredEnding))
            {
                return data;
            }
            else
            {
                currentID = data.FailID;
            }
        }

        return null;
    }

    private bool CheckIfEndingCleared(string endingName)
    {
        // TODO
        return false;
    }

    public void StartDialogue(string startID, Action<string> onDialogueEnd = null)
    {
        _onDialogueEndCallBack = onDialogueEnd;
        _initialStartID = startID;
        _isWaitingForChoice = false;

        _currentDialogue = GetValidDialogue(startID);

        if (_currentDialogue == null)
        {
            Debug.LogWarning($"[DialogueManager] 유효한 대화 ID를 찾을 수 없습니다. 요청ID: {startID}");
            return;
        }

        if (_currentPopup == null)
        {
            _currentPopup = SingletonManagers.UI.ShowPopupUI<UI_Popup_Dialogue>("UI_Popup_Dialogue");
        }

        if (!_currentPopup.gameObject.activeSelf)
        {
            _currentPopup.gameObject.SetActive(true);
        }

        _currentPopup.SetDialogue(_currentDialogue);
    }

    public void ProcessNextDialogue()
    {
        if (_currentDialogue == null) return;
        if (_isWaitingForChoice) return;

        // 입력창이 필요한 대사라면 입력창을 띄운다
        if (!string.IsNullOrEmpty(_currentDialogue.InputKey))
        {
            _isWaitingForChoice = true; // 입력이 끝날 때까지 대화 진행을 멈춤

            // 대화창 잠시 숨기기
            if (_currentPopup != null) _currentPopup.gameObject.SetActive(false);

            // 입력 팝업 띄우기
            var inputPopup = SingletonManagers.UI.ShowPopupUI<UI_Popup_Input>("UI_Popup_Input");
            inputPopup.Setup((inputValue) => 
            {
                // 유저가 입력을 마치고 확인을 누르면 실행됨
                StringVariables[_currentDialogue.InputKey] = inputValue; // 딕셔너리에 저장
                _isWaitingForChoice = false;

                // 다음 대사로 진행
                AdvanceToDialogue(_currentDialogue.NextID);
            }, _currentDialogue.InputQuestion);
            return;
        }

        // 선택지가 존재할 경우 대화창을 끄고 선택창을 띄운다
        if (_currentDialogue.Choices != null && _currentDialogue.Choices.Count > 0)
        {
            _isWaitingForChoice = true;

            if (_currentPopup != null)
            {
                _currentPopup.gameObject.SetActive(false);
            }

            _currentSelectPopup = SingletonManagers.UI.ShowPopupUI<UI_Popup_Select>("UI_Popup_Select");
            _currentSelectPopup.Setup(_currentDialogue.Choices.ToArray(), OnChoiceSelected);
            return;
        }

        string nextID = _currentDialogue.NextID;

        if (string.IsNullOrEmpty(nextID) || nextID == "End")
        {
            EndDialogue();
        }
        else
        {
            AdvanceToDialogue(nextID);
        }
    }

    private void OnChoiceSelected(int selectedIndex)
    {
        _isWaitingForChoice = false;
        _currentSelectPopup = null;

        if (_currentDialogue == null) return;

        string nextID = _currentDialogue.ChoiceNextIDs[selectedIndex];

        if (string.IsNullOrEmpty(nextID) || nextID == "End")
        {
            EndDialogue();
        }
        else
        {
            AdvanceToDialogue(nextID);
        }
    }

    /// <summary>
    /// 다음 대화로 매끄럽게 넘어가게 하기 위한 함수
    /// </summary>
    private void AdvanceToDialogue(string nextID)
    {
        _currentDialogue = GetValidDialogue(nextID);

        if (_currentDialogue != null)
        {
            if (_currentPopup != null && !_currentPopup.gameObject.activeSelf)
            {
                _currentPopup.gameObject.SetActive(true);
            }
            else if (_currentPopup == null)
            {
                _currentPopup = SingletonManagers.UI.ShowPopupUI<UI_Popup_Dialogue>("UI_Popup_Dialogue");
            
            }
            _currentPopup.SetDialogue(_currentDialogue);
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] 유효한 다음 대화를 찾을 수 없어 대화를 종료합니다. 요청ID: {nextID}");
            EndDialogue();
        }
    }

    /// <summary>
    /// 엑셀에 써있는 이벤트 이름을 보고 이벤트를 실행해주는 함수
    /// </summary>
    public void TriggerDialogueEvent(string eventNames)
    {
        if (string.IsNullOrEmpty(eventNames)) return;

        string[] events = eventNames.Split(',');

        foreach (string evt in events)
        {
            string cleanEvent = evt.Trim();

            if (!string.IsNullOrEmpty(cleanEvent))
            {
                OnDialogueEvent?.Invoke(cleanEvent);
            }
        }
    }

    /// <summary>
    /// 대화가 끝났을 때 실행되는 함수
    /// </summary>
    public void EndDialogue()
    {
        if (_currentSelectPopup != null)
        {
            SingletonManagers.UI.ClosePopupUI(_currentSelectPopup);
            _currentSelectPopup = null;
        }

        if (_currentPopup != null)
        {
            SingletonManagers.UI.ClosePopupUI(_currentPopup);
            _currentPopup = null;
        }

        string idToSave = _initialStartID;

        if (_currentDialogue != null && !String.IsNullOrEmpty(_currentDialogue.NextStartID))
        {
            idToSave = _currentDialogue.NextStartID; // 다시 말 걸었을 때의 시작 대화의 ID
            _dialogueProgressDB[_initialStartID] = idToSave;
        }

        _onDialogueEndCallBack?.Invoke(idToSave);

        _currentDialogue = null;
        _isWaitingForChoice = false;

        OnDialogueEnd?.Invoke();
        ClearEvents();
    }

    public void ClearEvents()
    {
        OnDialogueEvent = null;
        OnDialogueEnd = null;
    }

    public void Clear()
    {
        _dialogueDB.Clear();
        _currentPopup = null;
        _currentSelectPopup = null;
        _isWaitingForChoice = false;
        ClearEvents();
    }

    public void OnDestroy()
    {
        //Clear();
        _dialogueProgressDB.Clear();
        _init = false;
    }
}