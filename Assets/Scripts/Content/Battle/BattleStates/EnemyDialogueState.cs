using UnityEngine;

public class EnemyDialogueState : BattleStateBase
{
    public override BattleStateType StateType => BattleStateType.EnemyDialogue;

    public override void Enter(BattleContext context)
    {
        // 쓸모없는 상태인듯...EnemyDialogue 상태 그냥 지울까 생각중
        context.Manager.ChangeState(BattleStateType.EnemyAttack);
    }
}