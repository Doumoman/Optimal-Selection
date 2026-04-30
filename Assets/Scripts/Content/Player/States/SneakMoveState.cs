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
        // 엎드리기 키 뗌 -> MoveState
        if (!data.isSneakHeld)
        {
            fsm.TransitionTo(fsm.MoveState);
            return;
        }

        // 점프 입력 -> AirborneState
        if (data.jumpRequested && data.isGrounded)
        {
            fsm.TransitionTo(fsm.AirborneState);
            return;
        }

        // 지면 이탈 -> AirborneState
        if (!data.isGrounded)
        {
            fsm.TransitionTo(fsm.AirborneState);
            return;
        }

        // TODO: 엎드린 상태에서 상호작용 키 누르면 물체 주움


        // 이동 처리, SneakSpeed 사용
        float horizontalVel = data.moveInput.x * data.sneakSpeed;
        fsm.SetVelocity(horizontalVel, 0f);

        // ── 애니메이션 ──
        // TODO
    }

    public override void Exit() { }
}
