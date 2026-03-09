using UnityEngine;

public class PlayerActionState : BattleStateBase
{
    public override BattleStateType StateType => BattleStateType.PlayerAction;

    public override void Enter(BattleContext context)
    {
        base.Enter(context);

        Debug.Log($"[PlayerActionState] 선택된 행동 : {context.SelectedActionType}");

        switch (context.SelectedActionType)
        {
            case BattleActionType.Attack:
                ExecuteAttack(context);
                break;

            case BattleActionType.Act:
                ExecuteAct(context);
                break;

            case BattleActionType.Item:
                ExecuteItem(context);
                break;

            case BattleActionType.Mercy:
                ExecuteMercy(context);
                break;

            default:
                ChangeState(context, BattleStateType.PlayerChoice);
                break;
        }
    }

    private void ExecuteAttack(BattleContext context)
    {
        // TODO:
        // 타이밍 바, 명중 판정, 데미지 연출 등
        context.DamageEnemy(context.PlayerAttack);

        if (context.IsBattleEndRequested)
            return;

        ChangeState(context, BattleStateType.EnemyDialogue);
    }

    private void ExecuteAct(BattleContext context)
    {
        // TODO:
        // ACT 결과에 따른 호감도/악 수치/대사 변화
        context.AddEvil(10);

        // 예시: 악이 가득 차면 강제 공격 상태로 넘김
        if (context.IsEvilMax())
        {
            Debug.Log("[PlayerActionState] 악이 최대치 도달 -> 강제 공격");
            context.SelectedActionType = BattleActionType.Attack;
            ChangeState(context, BattleStateType.PlayerAction);
            return;
        }

        ChangeState(context, BattleStateType.EnemyDialogue);
    }

    private void ExecuteItem(BattleContext context)
    {
        // TODO:
        // 아이템 처리
        context.PlayerCurrentHP = Mathf.Min(context.PlayerMaxHP, context.PlayerCurrentHP + 5);
        ChangeState(context, BattleStateType.EnemyDialogue);
    }

    private void ExecuteMercy(BattleContext context)
    {
        // TODO:
        // 자비 가능 여부 판정
        bool canMercy = false;

        if (canMercy)
        {
            context.RequestBattleEnd(BattleResult.Mercy);
            return;
        }

        ChangeState(context, BattleStateType.EnemyDialogue);
    }
}