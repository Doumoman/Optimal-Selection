public enum BattleStateType
{
    None = 0,
    PlayerChoice,
    PlayerAction,
    EnemyDialogue,
    EnemyAttack,
    TurnEnd
}
public enum BattleResult
{
    None = 0,
    Win,
    Lose,
    Mercy,
    Escape
}
public enum BattleActionType
{
    None = 0,
    Attack,
    Act,
    Item,
    Mercy
}