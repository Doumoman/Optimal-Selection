using UnityEngine;

public class SneakMoveState : PlayerBaseState
{
    public SneakMoveState(PlayerFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.SetVelocity(0f, 0f);
    }

    public override void Update()
    {
        // 엎드리기 해제 토글
        if (data.sneakToggled)
        {
            fsm.TransitionTo(fsm.MoveState);
            return;
        }

        // 지면 이탈 (엎드린 상태에서 낙하 가능)
        if (!data.isGrounded)
        {
            fsm.TransitionTo(fsm.AirborneState);
            return;
        }

        // 이동 처리, SneakSpeed 사용
        float horizontalVel = data.moveInput.x * data.sneakSpeed;
        fsm.SetVelocity(horizontalVel, 0f);

        // ── 애니메이션 ──
        // TODO
    }

    public override void Exit()
    {
        data.sneakToggled = false;
    }
}
