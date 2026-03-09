using UnityEngine;

public abstract class BattleStateBase : IBattleState
{
    public abstract BattleStateType StateType { get; }

    public virtual void Enter(BattleContext context)
    {
        Debug.Log($"[BattleState] Enter : {StateType}");
    }

    public virtual void Tick(BattleContext context)
    {
    }

    public virtual void Exit(BattleContext context)
    {
        Debug.Log($"[BattleState] Exit : {StateType}");
    }

    protected void ChangeState(BattleContext context, BattleStateType next)
    {
        context.Manager.ChangeState(next);
    }

    protected void PushAndChangeState(BattleContext context, BattleStateType returnState, BattleStateType next)
    {
        context.PushState(returnState);
        context.Manager.ChangeState(next);
    }
}