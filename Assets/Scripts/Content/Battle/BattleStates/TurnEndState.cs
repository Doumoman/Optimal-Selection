using UnityEngine;

public class TurnEndState : BattleStateBase
{
    public override BattleStateType StateType => BattleStateType.TurnEnd;

    public override void Enter(BattleContext context)
    {
        base.Enter(context);

        context.TurnIndex++;

        Debug.Log($"[TurnEndState] 턴 종료. 현재 턴: {context.TurnIndex}");

        if (context.IsBattleEndRequested)
            return;

        if (context.IsPlayerDead())
        {
            context.RequestBattleEnd(BattleResult.Lose);
            return;
        }

        if (context.IsEnemyDead())
        {
            context.RequestBattleEnd(BattleResult.Win);
            return;
        }

        ChangeState(context, BattleStateType.PlayerChoice);
    }
}