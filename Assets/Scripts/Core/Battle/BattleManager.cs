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

        EnemyBattleSO enemyData = LoadEnemyBattleData(request.enemyId);
        if (enemyData == null)
        {
            Debug.LogError($"[BattleManager] 적 전투 데이터를 찾을 수 없습니다. enemyId={request.enemyId}");
            return;
        }

        if (enemyData.defaultFlow == null)
        {
            Debug.LogError($"[BattleManager] EnemyBattleSO의 defaultFlow가 비어 있습니다. enemyId={request.enemyId}");
            return;
        }

        if (enemyData.battlePrefab == null)
        {
            Debug.LogWarning($"[BattleManager] EnemyBattleSO의 battlePrefab이 비어 있습니다. enemyId={request.enemyId}");
        }

        if (string.IsNullOrEmpty(enemyData.defaultFlow.startNodeId))
        {
            Debug.LogError($"[BattleManager] defaultFlow의 startNodeId가 비어 있습니다. flow={enemyData.defaultFlow.name}");
            return;
        }

        IsBattleRunning = true;

        Context.Init(this, request);
        Context.SceneRefs = _sceneRefs;
        Context.PatternRunner = _sceneRefs.patternRunner;

        Context.EnemyMaxHP = enemyData.maxHP;
        Context.EnemyCurrentHP = enemyData.maxHP;
        Context.CurrentFlow = enemyData.defaultFlow;

        PrepareWorldForBattle(request);
        SetupBattlePresentation(enemyData);

        if (_sceneRefs.battleRoot != null)
            _sceneRefs.battleRoot.SetActive(true);

        if (_sceneRefs.battleUIRoot != null)
            _sceneRefs.battleUIRoot.SetActive(true);

        Debug.Log($"[BattleManager] 전투 시작 enemyId={request.enemyId}, flow={enemyData.defaultFlow.name}, startNodeId={enemyData.defaultFlow.startNodeId}");

        ChangeState(BattleStateType.PlayerChoice);
    }
    private void SetupBattlePresentation(EnemyBattleSO enemyData)
    {
        if (_sceneRefs.battleRoot != null)
            _sceneRefs.battleRoot.SetActive(true);

        if (_sceneRefs.battleUIRoot != null)
            _sceneRefs.battleUIRoot.SetActive(true);

        if (_sceneRefs.battleFieldView != null)
        {
            _sceneRefs.battleFieldView.Show();

            if (enemyData.battlePrefab != null)
                _sceneRefs.battleFieldView.SpawnEnemy(enemyData.battlePrefab);

            GameObject soulPrefab = LoadDefaultSoulPrefab();
            if (soulPrefab != null)
                _sceneRefs.battleFieldView.SpawnSoul(soulPrefab);
            else
                Debug.LogWarning("[BattleManager] 기본 소울 프리팹을 찾지 못했습니다.");
        }
    }

    private EnemyBattleSO LoadEnemyBattleData(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId))
            return null;

        // Resources/Battle/Data/Enemy1.asset 형태를 가정
        // 여기 안에 SO를 넣고 데이터를 받아주면 된다.
        return SingletonManagers.Resource.Load<EnemyBattleSO>($"Battle/Enemy/{enemyId}");
    }


    private GameObject LoadDefaultSoulPrefab()
    {
        return SingletonManagers.Resource.Load<GameObject>("Battle/Player/PlayerSoul");
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
            portrait = SingletonManagers.Resource.Load<Sprite>($"Portraits/{data.PortraitName}");

        Context.SceneRefs.enemySpeechUI.Show(data.Speaker, data.Text, portrait, onComplete);
    }

    public void ShowSystemDialogue(string dialogueId, Action<string> onEnd = null)
    {
        SingletonManagers.Dialogue.StartDialogue(dialogueId, onEnd);
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

            if (_sceneRefs.battleFieldView != null)
                _sceneRefs.battleFieldView.Hide();

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
        if (_sceneRefs != null && _sceneRefs.playerFSM != null && request.lockWorldInput)
        {
            // _sceneRefs.playerController.SetInputLock(true);
        }
    }

    private void RestoreWorldAfterBattle(BattleResult result)
    {
        if (_sceneRefs != null && _sceneRefs.playerFSM != null)
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