using UnityEngine;

/// <summary>
/// 플레이어 FSM 상태 추상 기반 클래스.
/// </summary>
public abstract class PlayerBaseState
{
    protected PlayerFSM fsm;
    protected PlayerData data;
    protected Animator anim;

    public PlayerBaseState(PlayerFSM fsm)
    {
        this.fsm = fsm;
        data = fsm.PlayerData;
        anim = fsm.Anim;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();

    protected void PlayClip(string clipName)
    {
        if (!string.IsNullOrEmpty(clipName))
            anim.Play(clipName);
    }

    protected void PlayAnimation(PlayerBaseState state)
    {
        int dir = fsm.GetDirectionIndex();
        string clip = null;

        if (state == fsm.MoveState)
        {
            bool isMoving = Mathf.Abs(data.moveHorizontalInput.x) > 0.01f;
            // TODO : 방향에 따른 애니메이션 출력
        }
        else if (state == fsm.SneakMoveState)
        {
            bool isMoving = Mathf.Abs(data.moveHorizontalInput.x) > 0.01f;
            // TODO : 방향에 따른 애니메이션 출력

        }
        else if (state == fsm.AirborneState) { clip = AnimClips.Airborne; }
        else if (state == fsm.LadderState) { clip = AnimClips.Ladder; }
        else if (state == fsm.HangState) { clip = AnimClips.Hanging; }
        else if (state == fsm.KilledState) { clip = AnimClips.Death; }

        if (!string.IsNullOrEmpty(clip))
            anim.Play(clip);
    }
}

public static class AnimClips
{
    public const string Idle = "Idle";
    public const string Walk = "Walk";
    public const string Jump = "Jump";
    public const string Airborne = "Airborne";
    public const string Hanging = "Hanging";
    public const string SneakIdle = "SneakIdle";
    public const string SneakWalk = "SneakWalk";
    public const string Ladder = "Ladder";
    public const string Death = "Death";
}