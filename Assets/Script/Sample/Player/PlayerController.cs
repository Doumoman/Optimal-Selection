using UnityEngine;
using UnityEngine.UIElements;
public class PlayerController : MonoBehaviour
{
    [Header("Player States")]
    public PlayerBaseState idleState;
    public PlayerBaseState sneakState;
    public PlayerBaseState sneakMoveState;
    public PlayerBaseState walkState;
    public PlayerBaseState specialState;

    private PlayerBaseState currentState;

    [Header("References")]
    public Animator anim;
    public Rigidbody2D rb;
    public BoxCollider2D bc;

    public float moveSpeed = 5f;
    public Vector2 moveDir;
    public Vector2 lastDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        idleState = new IdleState(this);
        sneakState = new SneakState(this);
        sneakMoveState = new SneakMoveState(this);
        walkState = new WalkState(this);
        specialState = new SpecialState(this);

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();

        lastDir = Vector2.down;
        ChangeState(idleState);
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveDir = new Vector2(h, v).normalized; // 방향벡터 정규화

        if (moveDir != Vector2.zero)
        {
            lastDir = moveDir;
            Vector2 currentScale = transform.localScale;

            if (moveDir.x > 0)
            {
                currentScale.x = Mathf.Abs(currentScale.x);
            }
            else
            {
                currentScale.x = -Mathf.Abs(currentScale.x);
            }

            transform.localScale = currentScale;
        }
        currentState?.Update();
    }

    public void ChangeState(PlayerBaseState newState)
    {
        currentState?.Exit(); // 기존 상태 벗어남
        currentState = newState; // 상태 변경
        newState?.Enter(); // 새로운 상태 진입
    }

    public int GetDirectionIndex()
    {
        if (lastDir.y > 0) return 0; // Back
        else if (lastDir.y < 0) return 1; // Front
        else return 2; // Side
    }
}

#region 플레이어 상태 로직
public class IdleState : PlayerBaseState
{
    public IdleState(PlayerController pc) : base(pc) { }

    public override void Enter()
    {

    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerContext.ChangeState(playerContext.sneakState);
            return;
        }

        if (playerContext.moveDir != Vector2.zero)
        {
            playerContext.ChangeState(playerContext.walkState);
            return;
        }
        PlayAnimation(this);
    }

    public override void Exit()
    {
        
    }
}

public class SneakState : PlayerBaseState
{
    public SneakState(PlayerController pc) : base(pc) { }

    public override void Enter()
    {

    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerContext.ChangeState(playerContext.idleState);
            return;
        }

        if(playerContext.moveDir != Vector2.zero)
        {
            playerContext.ChangeState(playerContext.sneakMoveState);
            return;
        }

        PlayAnimation(this);
    }

    public override void Exit()
    {

    }
}

public class SneakMoveState : PlayerBaseState
{
    public SneakMoveState(PlayerController pc) : base(pc) { }

    public override void Enter()
    {

    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerContext.ChangeState(playerContext.walkState);
            return;
        }

        if (playerContext.moveDir == Vector2.zero)
        {
            playerContext.ChangeState(playerContext.sneakState);
            return;
        }

        PlayAnimation(this);

        float sneakSpeed = playerContext.moveSpeed / 2.0f;
        playerContext.transform.Translate(playerContext.moveDir * sneakSpeed * Time.deltaTime);
    }

    public override void Exit()
    {

    }
}

public class WalkState : PlayerBaseState
{
    public WalkState(PlayerController pc) : base(pc) { }

    public override void Enter()
    {
        
    }

    public override void Update()
    {
        if (playerContext.moveDir == Vector2.zero)
        {
            playerContext.ChangeState(playerContext.idleState);
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerContext.ChangeState(playerContext.sneakMoveState);
            return;
        }

        PlayAnimation(this);

        playerContext.transform.Translate(playerContext.moveDir * playerContext.moveSpeed * Time.deltaTime);
    }

    public override void Exit()
    {

    }
}

public class SpecialState : PlayerBaseState
{
    public SpecialState(PlayerController pc) : base(pc) { }

    public override void Enter()
    {

    }
    public override void Update()
    {

    }

    public override void Exit()
    {

    }
}
#endregion