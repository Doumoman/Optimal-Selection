using UnityEngine;
/// <summary>
/// 샘플용 상태머신
/// </summary>
public abstract class PlayerBaseState
{
    protected PlayerController playerContext;

    public PlayerBaseState(PlayerController playerContext) {  this.playerContext = playerContext; }
    public abstract void Enter(); // 상태 진입 시 1회 실행
    public abstract void Update(); // 매 프레임마다 실행
    public abstract void Exit(); // 상태 종료 시 1회 실행

    protected void PlayAnimation(PlayerBaseState state)
    {
        int dirIndex = playerContext.GetDirectionIndex();
        string clip = null;

        if (state == playerContext.idleState)
        {
            clip = dirIndex switch
            {
                0 => "Idle_Back",
                1 => "Idle_Front",
                _ => "Idle_Side"
            };
        }

        if (state == playerContext.sneakState)
        {
            clip = dirIndex switch
            {
                0 => "Sneak_Back",
                1 => "Sneak_Front",
                _ => "Sneak_Side"
            };
        }

        if (state == playerContext.sneakMoveState)
        {
            clip = dirIndex switch
            {
                0 => "SneakMove_Back",
                1 => "SneakMove_Front",
                _ => "SneakMove_Side"
            };
        }

        if (state == playerContext.walkState)
        {
            clip = dirIndex switch
            {
                0 => "Walk_Back",
                1 => "Walk_Front",
                _ => "Walk_Side"
            };
        }

        if (state == playerContext.specialState)
        {

        }

        playerContext.anim.Play(clip); // 애니메이션 실행
    }
}
