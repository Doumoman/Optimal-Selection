using UnityEngine;

public class LadderState : PlayerBaseState
{
    public LadderState(PlayerFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.SetVelocity(0f, 0f);
    }

    public override void Update()
    {
        if (!data.isNearLadder)
        {
            // 사다리 점프 이탈 (공중으로 나감) → AirborneState
            // 사다리 하단 이탈 (땅에 닿음) → MoveState
            if (data.isGrounded)
                fsm.TransitionTo(fsm.MoveState);
            else
                fsm.TransitionTo(fsm.AirborneState);
            return;
        }

        // 이동 처리, ladderSpeed 사용
        float verticalVel = data.moveInput.y * data.ladderSpeed;
        fsm.SetVelocity(0f, verticalVel);

        // ── 애니메이션 ──
        // TODO
    }

    public override void Exit()
    {
        fsm.SetVelocity(0f, 0f);
    }
}
