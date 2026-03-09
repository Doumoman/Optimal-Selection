public interface IBattleState
{
    BattleStateType StateType { get; }

    void Enter(BattleContext context);
    void Tick(BattleContext context);
    void Exit(BattleContext context);
}
