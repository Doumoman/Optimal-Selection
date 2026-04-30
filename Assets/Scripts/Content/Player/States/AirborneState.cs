using UnityEngine;

public class AirborneState : PlayerBaseState
{
    private float jumpHoldTime = 0f;
    private bool isJumping = false;

    public AirborneState(PlayerFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        jumpHoldTime = 0f;

        if (data.jumpRequested)
        {
            isJumping = true;
            fsm.SetVelocityY(data.jumpSpeed);
        }
        else
        {
            isJumping = false;
        }
    }

    public override void Update()
    {
        Vector2 vel = fsm.GetVelocity();

        float effectiveGravity;
        if (isJumping && data.isJumpHeld && jumpHoldTime < data.jumpMaxHoldTime) // 짧게 점프했을 경우
        {
            effectiveGravity = data.gravity * data.jumpHoldGravityScale;
            jumpHoldTime += Time.deltaTime;
        }
        else // 낙하
        {
            isJumping = false;
            effectiveGravity = data.gravity;
        }

        vel.y += effectiveGravity * Time.deltaTime;
        vel.y = Mathf.Max(vel.y, data.maxFallSpeed);
        vel.x = data.moveInput.x * data.moveSpeed;

        fsm.SetVelocity(vel.x, vel.y);

        // 착지 → MoveState
        if (data.isGrounded && vel.y <= 0f)
        {
            fsm.TransitionTo(fsm.MoveState);
            return;
        }

        // 사다리 + 세로 입력 → LadderState
        if (data.isNearLadder && Mathf.Abs(data.moveInput.y) > 0.001f)
        {
            fsm.TransitionTo(fsm.LadderState);
            return;
        }

        // ── 애니메이션 ──
        // TODO
    }

    public override void Exit() { }
}