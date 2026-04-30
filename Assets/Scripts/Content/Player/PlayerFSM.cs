using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
public class PlayerFSM : MonoBehaviour
{
    [Header("References")]
    public Animator Anim { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public BoxCollider2D Bc { get; private set; }

    [SerializeField] private PlayerData _playerData = new PlayerData();
    public PlayerData PlayerData => _playerData;

    [Header("States")]
    private PlayerBaseState _currentState;
    public PlayerBaseState MoveState { get; private set; }
    public PlayerBaseState AirborneState { get; private set; }
    public PlayerBaseState LadderState { get; private set; }
    public PlayerBaseState SneakMoveState { get; private set; }
    public PlayerBaseState PushState { get; private set; }
    public PlayerBaseState KilledState { get; private set; }

    [Header("Physics")]
    private Vector2 _velocity; // 속도
    public Vector2 lastDir; // 마지막 방향

    private readonly float SkinWidth = 0.02f;
    private readonly int RayCount = 3;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Rb = GetComponent<Rigidbody2D>();
        Bc = GetComponent<BoxCollider2D>();

        Rb.bodyType = RigidbodyType2D.Kinematic;
        Rb.gravityScale = 0f;
    }

    private void Start()
    {
        MoveState = new MoveState(this);
        AirborneState = new AirborneState(this);
        LadderState = new LadderState(this);
        SneakMoveState = new SneakMoveState(this);
        PushState = new PushState(this);
        KilledState = new KilledState(this);

        // ㅡㅡㅡㅡ InputManager에서 이벤트 구독 ㅡㅡㅡㅡ
        SingletonManagers.Input.OnMove -= HandleMoveInput;
        SingletonManagers.Input.OnMove += HandleMoveInput;

        SingletonManagers.Input.OnLadderMove -= HandleLadderMoveInput;
        SingletonManagers.Input.OnLadderMove += HandleLadderMoveInput;

        SingletonManagers.Input.OnJumpPressed -= HandleJump;
        SingletonManagers.Input.OnJumpPressed += HandleJump;

        SingletonManagers.Input.OnJumpReleased -= HandleJumpReleased;
        SingletonManagers.Input.OnJumpReleased += HandleJumpReleased;

        SingletonManagers.Input.OnSneakPressed -= HandleSneak;
        SingletonManagers.Input.OnSneakPressed += HandleSneak;

        SingletonManagers.Input.OnSneakReleased -= HandleSneakReleased;
        SingletonManagers.Input.OnSneakReleased += HandleSneakReleased;

        lastDir = Vector2.right;
        TransitionTo(MoveState); // 초기 상태
    }


    private void Update()
    {
        // 사망 일괄 처리
        if (_playerData.isDead && _currentState != KilledState)
        {
            TransitionTo(KilledState);
            return;
        }

        // 지면 감지 (전이 판정 전에 갱신)
        CheckGround();
        CheckLadder();

        // lastDir 갱신 및 스프라이트 좌우 플리핑
        if (_playerData.moveHorizontalInput != Vector2.zero)
        {
            lastDir = _playerData.moveHorizontalInput;
            Vector2 scale = transform.localScale;
            scale.x = _playerData.moveHorizontalInput.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        _currentState?.Update();
        ApplyMovement();
    }

    private void LateUpdate()
    {
        // 1프레임 소비 플래그 초기화
        _playerData.jumpRequested = false;
    }

    // ㅡㅡ 상태 전이 함수 ㅡㅡ
    public void TransitionTo(PlayerBaseState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    // ㅡㅡ 스프라이트 좌우 반전을 위한 함수 ㅡㅡ
    public int GetDirectionIndex()
    {
        if (lastDir.x > 0) return 1; // right
        else if (lastDir.x < 0) return -1; // left
        else return 0;
    }

    public void SetVelocity(float x, float y) => _velocity = new Vector2(x, y);
    public void SetVelocityX(float x) => _velocity.x = x;
    public void SetVelocityY(float y) => _velocity.y = y;
    public Vector2 GetVelocity() => _velocity;

    // ── 이동 + 충돌 처리 ──
    private void ApplyMovement()
    {
        Vector2 delta = _velocity * Time.deltaTime;
        delta = ResolveHorizontal(delta); // 좌우 충돌 보정
        delta = ResolveVertical(delta); // 상하 충돌 보정
        transform.position += (Vector3)delta;
    }

    private Vector2 ResolveHorizontal(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) < 0.0001f) return delta;

        float dir = Mathf.Sign(delta.x);
        float rayLength = Mathf.Abs(delta.x) + SkinWidth;
        float halfH = Bc.size.y * 0.5f - SkinWidth;
        Vector2 center = (Vector2)transform.position + Bc.offset;

        for (int i = 0; i < RayCount; i++)
        {
            float t = (RayCount == 1) ? 0.5f : (float)i / (RayCount - 1);
            Vector2 origin = center + Vector2.up * Mathf.Lerp(-halfH, halfH, t);
            origin.x += dir * (Bc.size.x * 0.5f - SkinWidth);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, rayLength, _playerData.groundLayer);
            if (hit.collider != null)
            {
                delta.x = (hit.distance - SkinWidth) * dir;
                SetVelocityX(0f);
                break;
            }
        }
        return delta;
    }

    private Vector2 ResolveVertical(Vector2 delta)
    {
        if (Mathf.Abs(delta.y) < 0.0001f) return delta;

        float dir = Mathf.Sign(delta.y);
        float rayLength = Mathf.Abs(delta.y) + SkinWidth;
        float halfW = Bc.size.x * 0.5f - SkinWidth;
        Vector2 center = (Vector2)transform.position + Bc.offset;

        for (int i = 0; i < RayCount; i++)
        {
            float t = (RayCount == 1) ? 0.5f : (float)i / (RayCount - 1);
            Vector2 origin = center + Vector2.right * Mathf.Lerp(-halfW, halfW, t);
            origin.y += dir * (Bc.size.y * 0.5f - SkinWidth);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * dir, rayLength, _playerData.groundLayer);
            if (hit.collider != null)
            {
                delta.y = (hit.distance - SkinWidth) * dir;
                SetVelocityY(0f);
                break;
            }
        }
        return delta;
    }

    // ── 지면 감지 ──
    private void CheckGround()
    {
        Vector2 center = (Vector2)transform.position + Bc.offset;
        float halfW = Bc.size.x * 0.5f - SkinWidth;

        for (int i = 0; i < RayCount; i++)
        {
            float t = (RayCount == 1) ? 0.5f : (float)i / (RayCount - 1);
            Vector2 origin = center + Vector2.right * Mathf.Lerp(-halfW, halfW, t);
            origin.y -= Bc.size.y * 0.5f - SkinWidth;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _playerData.groundCheckDistance + SkinWidth, _playerData.groundLayer);
            if (hit.collider != null)
            {
                _playerData.isGrounded = true;
                return;
            }
        }
        _playerData.isGrounded = false;
    }

    // ── 사다리 감지 ──
    private void CheckLadder()
    {
        Vector2 center = (Vector2)transform.position + Bc.offset;
        _playerData.isNearLadder = Physics2D.OverlapPoint(center, _playerData.ladderLayer);
    }

    // ── 입력 핸들러 ──
    private void HandleMoveInput(Vector2 dir) => _playerData.moveHorizontalInput = dir;
    private void HandleLadderMoveInput(Vector2 dir) => _playerData.MoveVerticalInput = dir;
    private void HandleJump()
    {
        _playerData.jumpRequested = true;
        _playerData.isJumpHeld = true;
    }
    private void HandleJumpReleased() => _playerData.isJumpHeld = false;
    private void HandleSneak() => _playerData.isSneakHeld = true;
    private void HandleSneakReleased() => _playerData.isSneakHeld = false;

    private void OnDestroy()
    {
        if (SingletonManagers.Input == null) return;

        SingletonManagers.Input.OnMove -= HandleMoveInput;
        SingletonManagers.Input.OnLadderMove -= HandleLadderMoveInput;

        SingletonManagers.Input.OnJumpPressed -= HandleJump;
        SingletonManagers.Input.OnJumpReleased -= HandleJumpReleased;

        SingletonManagers.Input.OnSneakPressed -= HandleSneak;
        SingletonManagers.Input.OnSneakReleased -= HandleSneakReleased;
    }
}
