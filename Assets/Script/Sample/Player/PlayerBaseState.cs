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
}
