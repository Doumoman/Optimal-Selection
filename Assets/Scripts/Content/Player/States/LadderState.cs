using UnityEngine;

public class LadderState : PlayerBaseState
{
    public LadderState(PlayerFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        fsm.SetVelocity(0f, 0f);
        Debug.Log("진입!");
    }

    public override void Update()
    {
        if (data.isNearLadder)
        {
            // 사다리 하단 이탈 (땅에 닿음) → MoveState
            // TODO: 바닥 판정과 사다리 판정 우선순위
            // TODO: 사다리 끝 처리
            if (data.isGrounded)
            {
                fsm.TransitionTo(fsm.MoveState);
                return;
            }

            // 사다리 점프 이탈 (공중으로 나감) → AirborneState
            if (data.jumpRequested)
            {
                fsm.TransitionTo(fsm.AirborneState);
                return;
            }

            // 이동 처리, ladderSpeed 사용
            float verticalVel = data.MoveVerticalInput.y * data.ladderSpeed;
            fsm.SetVelocity(0f, verticalVel);

            // ── 애니메이션 ──
            // TODO
        }
        else
        {
            fsm.TransitionTo(fsm.MoveState);
        }
    }

    public override void Exit()
    {
        fsm.SetVelocity(0f, 0f);
    }
}
