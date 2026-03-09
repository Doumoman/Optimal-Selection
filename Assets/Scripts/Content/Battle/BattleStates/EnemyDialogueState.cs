using UnityEngine;

public class EnemyDialogueState : BattleStateBase
{
    public override BattleStateType StateType => BattleStateType.EnemyDialogue;

    public override void Enter(BattleContext context)
    {
        base.Enter(context);

        string key = $"Turn_{context.TurnIndex + 1:00}";
        context.Manager.ShowEnemySpeechByKey(key, () =>
        {
            context.Manager.ChangeState(BattleStateType.EnemyAttack);
        });
    }
}