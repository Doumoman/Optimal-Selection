using System.Collections.Generic;
using UnityEngine;

public class BattleContext
{
    public BattleFlowSO CurrentFlow { get; set; }
    public PatternRunner PatternRunner { get; set; }

    public BattleManager Manager { get; private set; }
    public BattleStartRequest StartRequest { get; private set; }

    public BattleStateType CurrentStateType { get; set; } = BattleStateType.None;
    public BattleStateType PreviousStateType { get; set; } = BattleStateType.None;

    private Stack<BattleStateType> _stateStack = new Stack<BattleStateType>();

    public int TurnIndex { get; set; }
    public bool IsBattleEndRequested { get; set; }
    public BattleResult PendingResult { get; set; }

    public BattleActionType SelectedActionType { get; set; }
    public int SelectedActIndex { get; set; }
    public int SelectedItemIndex { get; set; }

    public int PlayerCurrentHP { get; set; }
    public int PlayerMaxHP { get; set; }
    public int PlayerAttack { get; set; }
    public int PlayerDefense { get; set; }

    public int EnemyCurrentHP { get; set; }
    public int EnemyMaxHP { get; set; }

    public int CurrentEvilGauge { get; set; }
    public int MaxEvilGauge { get; set; }


    public BattleSceneRefs SceneRefs { get; set; }

    public string EnemyId => StartRequest != null ? StartRequest.enemyId : string.Empty;

    public void Init(BattleManager manager, BattleStartRequest request)
    {
        Manager = manager;
        StartRequest = request;

        CurrentStateType = BattleStateType.None;
        PreviousStateType = BattleStateType.None;
        _stateStack.Clear();

        TurnIndex = 0;
        IsBattleEndRequested = false;
        PendingResult = BattleResult.None;

        SelectedActionType = BattleActionType.None;
        SelectedActIndex = -1;
        SelectedItemIndex = -1;

        PlayerCurrentHP = 20;
        PlayerMaxHP = 20;
        PlayerAttack = 5;
        PlayerDefense = 1;

        EnemyCurrentHP = 50;
        EnemyMaxHP = 50;

        CurrentEvilGauge = 0;
        MaxEvilGauge = 100;
    }

    public void ClearSelections()
    {
        SelectedActionType = BattleActionType.None;
        SelectedActIndex = -1;
        SelectedItemIndex = -1;
    }

    public void PushState(BattleStateType stateType) => _stateStack.Push(stateType);

    public bool TryPopState(out BattleStateType stateType)
    {
        if (_stateStack.Count > 0)
        {
            stateType = _stateStack.Pop();
            return true;
        }

        stateType = BattleStateType.None;
        return false;
    }

    public void RequestBattleEnd(BattleResult result)
    {
        IsBattleEndRequested = true;
        PendingResult = result;
    }

    public bool IsPlayerDead() => PlayerCurrentHP <= 0;
    public bool IsEnemyDead() => EnemyCurrentHP <= 0;
    public bool IsEvilMax() => CurrentEvilGauge >= MaxEvilGauge;

    public void AddEvil(int value)
    {
        CurrentEvilGauge = Mathf.Clamp(CurrentEvilGauge + value, 0, MaxEvilGauge);
    }

    public void DamagePlayer(int damage)
    {
        int finalDamage = Mathf.Max(1, damage - PlayerDefense);
        PlayerCurrentHP = Mathf.Max(0, PlayerCurrentHP - finalDamage);

        if (PlayerCurrentHP <= 0)
            RequestBattleEnd(BattleResult.Lose);
    }

    public void DamageEnemy(int damage)
    {
        int finalDamage = Mathf.Max(1, damage);
        EnemyCurrentHP = Mathf.Max(0, EnemyCurrentHP - finalDamage);

        if (EnemyCurrentHP <= 0)
            RequestBattleEnd(BattleResult.Win);
    }
}