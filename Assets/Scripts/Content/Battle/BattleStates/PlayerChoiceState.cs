using UnityEngine;

public class PlayerChoiceState : BattleStateBase
{
    public override BattleStateType StateType => BattleStateType.PlayerChoice;

    public override void Enter(BattleContext context)
    {
        base.Enter(context);

        context.ClearSelections();

        // TODO:
        // - Fight / Act / Item / Mercy 메뉴 표시
        // - 커서 활성화
        // - 플레이어 입력 가능

        Debug.Log("[PlayerChoiceState] 행동 선택 대기");
    }

    public override void Tick(BattleContext context)
    {
        // 임시 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            context.SelectedActionType = BattleActionType.Attack;
            ChangeState(context, BattleStateType.PlayerAction);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            context.SelectedActionType = BattleActionType.Act;
            ChangeState(context, BattleStateType.PlayerAction);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            context.SelectedActionType = BattleActionType.Item;
            ChangeState(context, BattleStateType.PlayerAction);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            context.SelectedActionType = BattleActionType.Mercy;
            ChangeState(context, BattleStateType.PlayerAction);
        }
    }
}