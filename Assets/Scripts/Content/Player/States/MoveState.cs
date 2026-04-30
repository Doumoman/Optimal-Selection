using UnityEngine;

public class MoveState : PlayerBaseState
{
    public MoveState(PlayerFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.SetVelocity(0f, 0f);
    }

    public override void Update()
    {
        // 점프 입력 -> AirborneState
        if (data.jumpRequested && data.isGrounded)
        {
            fsm.TransitionTo(fsm.AirborneState);
            return;
        }

        // 낙하 - > AirborneState
        if (!data.isGrounded)
        {
            fsm.TransitionTo(fsm.AirborneState);
            return;
        }

        // 사다리 감지 + 윗 방향키 -> LadderState
        if (data.isNearLadder && Mathf.Abs(data.moveInput.y) > 0.001f)
        {
            fsm.TransitionTo(fsm.LadderState);
            return;
        }

        // 밀 수 있는 물체 감지 -> PushState
        if (data.isPushing)
        {
            fsm.TransitionTo(fsm.PushState);
            return;
        }

        // 아래 방향키 -> SneakMoveState
        if (data.sneakToggled)
        {
            fsm.TransitionTo(fsm.SneakMoveState);
            return;
        }

        // 이동 처리
        float targetVel = data.moveInput.x * data.moveSpeed;
        fsm.SetVelocity(targetVel, 0f); // x축에 대해서만 이동


        // ── 애니메이션 ──
        // TODO
    }

    public override void Exit() { }
}
