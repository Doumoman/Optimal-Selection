using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : IManager
{
    private bool _init = false;

    private readonly Dictionary<BattleStateType, IBattleState> _states = new Dictionary<BattleStateType, IBattleState>();
    private readonly BattleDialogueDatabase _battleDialogueDB = new BattleDialogueDatabase();

    private BattleSceneRefs _sceneRefs;

    public BattleContext Context { get; private set; } = new BattleContext();
    public bool IsBattleRunning { get; private set; }

    public event Action<BattleResult> OnBattleFinished;

    public void Init()
    {
        if (_init) return;
        _init = true;

        RegisterStates();

        // 필요하면 DataManager에서 통합 호출해도 됨
        _battleDialogueDB.Load("BattleDialogue");
    }

    public void Tick()
    {
        if (!IsBattleRunning)
            return;

        if (_states.TryGetValue(Context.CurrentStateType, out var state))
        {
            state.Tick(Context);
        }

        if (Context.IsBattleEndRequested)
        {
            HandleBattleEnd(Context.PendingResult);
        }
    }

    public void RegisterScene(BattleSceneRefs refs)
    {
        _sceneRefs = refs;

        if (_sceneRefs != null)
        {
            if (_sceneRefs.battleRoot != null)
                _sceneRefs.battleRoot.SetActive(false);

            if (_sceneRefs.battleUIRoot != null)
                _sceneRefs.battleUIRoot.SetActive(false);

            if (_sceneRefs.enemySpeechUI != null)
                _sceneRefs.enemySpeechUI.HideImmediate();
        }
    }

    public void StartBattle(BattleStartRequest request)
    {
        if (IsBattleRunning)
            return;

        if (_sceneRefs == null)
        {
            Debug.LogError("[BattleManager] SceneRefs가 없습니다.");
            return;
        }

        IsBattleRunning = true;

        Context.Init(this, request);
        Context.SceneRefs = _sceneRefs;
        Context.PatternRunner = _sceneRefs.patternRunner;

        Context.CurrentFlow = LoadBattleFlow(request.flowId);

        if (Context.CurrentFlow == null)
        {
            Debug.LogError($"[BattleManager] 시작 플로우를 찾을 수 없습니다. flowId={request.flowId}");
            IsBattleRunning = false;
            return;
        }

        if (_sceneRefs.battleRoot != null)
            _sceneRefs.battleRoot.SetActive(true);

        if (_sceneRefs.battleUIRoot != null)
            _sceneRefs.battleUIRoot.SetActive(true);

        ChangeState(BattleStateType.PlayerChoice);
    }

    private BattleFlowSO LoadBattleFlow(string flowId)
    {
        if (string.IsNullOrEmpty(flowId))
            return null;

        // 예시 경로: Resources/Battle/Flow/{flowId}
        return Managers.Resource.Load<BattleFlowSO>($"Battle/Flow/{flowId}");
    }

    public void ChangeState(BattleStateType nextStateType)
    {
        if (!IsBattleRunning)
            return;

        if (Context.CurrentStateType == nextStateType)
            return;

        if (_states.TryGetValue(Context.CurrentStateType, out var currentState))
            currentState.Exit(Context);

        Context.PreviousStateType = Context.CurrentStateType;
        Context.CurrentStateType = nextStateType;

        if (_states.TryGetValue(nextStateType, out var nextState))
            nextState.Enter(Context);
        else
            Debug.LogError($"[BattleManager] 등록되지 않은 상태: {nextStateType}");
    }

    public BattleDialogueData GetBattleDialogue(string key)
    {
        return _battleDialogueDB.GetByEnemyAndKey(Context.EnemyId, key);
    }

    public void ShowEnemySpeechByKey(string key, Action onComplete = null)
    {
        BattleDialogueData data = GetBattleDialogue(key);

        if (data == null)
        {
            Debug.LogWarning($"[BattleManager] 전투 대사 없음 Enemy={Context.EnemyId}, Key={key}");
            onComplete?.Invoke();
            return;
        }

        ShowEnemySpeech(data, onComplete);
    }

    public void ShowEnemySpeech(BattleDialogueData data, Action onComplete = null)
    {
        if (Context.SceneRefs == null || Context.SceneRefs.enemySpeechUI == null || data == null)
        {
            onComplete?.Invoke();
            return;
        }

        Sprite portrait = null;
        if (!string.IsNullOrEmpty(data.PortraitName))
            portrait = Managers.Resource.Load<Sprite>($"Portraits/{data.PortraitName}");

        Context.SceneRefs.enemySpeechUI.Show(data.Speaker, data.Text, portrait, onComplete);
    }

    public void ShowSystemDialogue(string dialogueId, Action<string> onEnd = null)
    {
        Managers.Dialogue.StartDialogue(dialogueId, onEnd);
    }

    public void FinishBattle(BattleResult result)
    {
        if (!IsBattleRunning)
            return;

        IsBattleRunning = false;

        if (_states.TryGetValue(Context.CurrentStateType, out var currentState))
            currentState.Exit(Context);

        if (_sceneRefs != null)
        {
            if (_sceneRefs.enemySpeechUI != null)
                _sceneRefs.enemySpeechUI.HideImmediate();

            if (_sceneRefs.battleUIRoot != null)
                _sceneRefs.battleUIRoot.SetActive(false);

            if (_sceneRefs.battleRoot != null)
                _sceneRefs.battleRoot.SetActive(false);
        }

        RestoreWorldAfterBattle(result);
        OnBattleFinished?.Invoke(result);
    }

    private void HandleBattleEnd(BattleResult result)
    {
        Context.IsBattleEndRequested = false;

        switch (result)
        {
            case BattleResult.Win:
                ShowSystemDialogue("Battle_Win", _ => FinishBattle(BattleResult.Win));
                break;
            case BattleResult.Mercy:
                ShowSystemDialogue("Battle_Mercy", _ => FinishBattle(BattleResult.Mercy));
                break;
            case BattleResult.Lose:
                ShowSystemDialogue("Battle_Lose", _ => FinishBattle(BattleResult.Lose));
                break;
            case BattleResult.Escape:
                ShowSystemDialogue("Battle_Escape", _ => FinishBattle(BattleResult.Escape));
                break;
            default:
                FinishBattle(result);
                break;
        }
    }

    private void RegisterStates()
    {
        _states.Clear();
        AddState(new PlayerChoiceState());
        AddState(new PlayerActionState());
        AddState(new EnemyDialogueState());
        AddState(new EnemyAttackState());
        AddState(new TurnEndState());
    }

    private void AddState(IBattleState state)
    {
        if (!_states.ContainsKey(state.StateType))
            _states.Add(state.StateType, state);
    }

    private void PrepareWorldForBattle(BattleStartRequest request)
    {
        if (_sceneRefs != null && _sceneRefs.playerController != null && request.lockWorldInput)
        {
            // _sceneRefs.playerController.SetInputLock(true);
        }
    }

    private void RestoreWorldAfterBattle(BattleResult result)
    {
        if (_sceneRefs != null && _sceneRefs.playerController != null)
        {
            // _sceneRefs.playerController.SetInputLock(false);
        }

        if (Context.StartRequest != null &&
            Context.StartRequest.enemyWorldObject != null &&
            (result == BattleResult.Win || result == BattleResult.Mercy))
        {
            UnityEngine.Object.Destroy(Context.StartRequest.enemyWorldObject);
        }
    }

    public void Clear()
    {
        IsBattleRunning = false;
        _sceneRefs = null;
        _battleDialogueDB.Clear();
    }

    public void OnDestroy()
    {
        Clear();
        _init = false;
    }
}