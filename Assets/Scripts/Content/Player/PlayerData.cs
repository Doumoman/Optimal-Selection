using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;
    public float sneakSpeed = 2.5f;
    public float ladderSpeed = 4f;
    public float pushSpeed = 2f;

    [Header("점프 / 중력")]
    public float jumpSpeed = 12f; // 점프 속도
    public float jumpMaxHoldTime = 0.3f; // 점프 키 최대 홀드 시간
    public float jumpHoldGravityScale = 0.3f; // 점프 시 가해지는 중력
    public float gravity = -25f; // 실제 중력
    public float maxFallSpeed = -20f; // 낙하 최고 스피드
    public float groundCheckDistance = 0.05f; // 커질수록 ground 감지 ray가 길어짐

    [Header("Layer 감지")]
    public LayerMask groundLayer;
    public LayerMask ladderLayer;
    public LayerMask hangerLayer;

    // ── 런타임 상태 ──
    public Vector2 moveHorizontalInput; // 입력받은 좌우 이동 방향
    public Vector2 MoveVerticalInput; // 입력받은 상하 이동 방향
    public bool jumpRequested; // 점프 요구
    public bool isJumpHeld;   // 점프 키 홀드 중
    public bool isSneakHeld; // 엎드리기 키 홀드 중
    public bool isGrounded; // 바닥에 있는지
    public bool isPushing; // 물체를 미는 중인지
    public bool isNearLadder; // 근처에 사다리가 있는지
    public bool isHolding; // 물체를 들고 있는지
    public bool isDead; // 플레이어 사망 여부
}
