using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PushableObject : MonoBehaviour, IPushable
{
    [SerializeField] private float gravity = -25f; // 중력
    [SerializeField] private float groundCheckDistance = 0.05f; // 커질수록 ground를 감지하는 ray가 길어짐
    [SerializeField] private float groundCheckInset = 0.1f; // ray를 오브젝트 가장자리에서 얼마나 안쪽에서 발사할지
    [SerializeField] private LayerMask solidLayer;

    private Rigidbody2D _rb;
    private BoxCollider2D _bc;
    private float _velocityX = 0f;
    private float _velocityY = 0f;

    private readonly float SkinWidth = 0.02f;
    private readonly int RayCount = 3;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;

        _rb.gravityScale = 0f;
        _bc = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (CheckGround())
            _velocityY = Mathf.Max(_velocityY, 0f);
        else
            _velocityY += gravity * Time.deltaTime;

        Vector2 delta = new Vector2(_velocityX, _velocityY) * Time.deltaTime;
        delta = ResolveVertical(delta);
        delta = ResolveHorizontal(delta);
        transform.position += (Vector3)delta;

        _velocityX = 0f;
    }

    public void SetHorizontalVelocity(float vx) => _velocityX = vx;

    private bool CheckGround()
    {
        Vector2 center = (Vector2)transform.position + _bc.offset;
        float halfW = _bc.size.x * 0.5f - groundCheckInset;

        for (int i = 0; i < RayCount; i++)
        {
            float t = (RayCount == 1) ? 0.5f : (float)i / (RayCount - 1);
            Vector2 origin = center + Vector2.right * Mathf.Lerp(-halfW, halfW, t);
            origin.y -= _bc.size.y * 0.5f - SkinWidth;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance + SkinWidth, solidLayer);
            if (hit.collider != null) return true;
        }
        return false;
    }

    private Vector2 ResolveVertical(Vector2 delta)
    {
        if (Mathf.Abs(delta.y) < 0.0001f) return delta;

        float dir = Mathf.Sign(delta.y);
        float rayLength = Mathf.Abs(delta.y) + SkinWidth;
        float halfW = _bc.size.x * 0.5f - SkinWidth;
        Vector2 center = (Vector2)transform.position + _bc.offset;

        for (int i = 0; i < RayCount; i++)
        {
            float t = (RayCount == 1) ? 0.5f : (float)i / (RayCount - 1);
            Vector2 origin = center + Vector2.right * Mathf.Lerp(-halfW, halfW, t);
            origin.y += dir * (_bc.size.y * 0.5f - SkinWidth);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * dir, rayLength, solidLayer);
            if (hit.collider != null)
            {
                delta.y = (hit.distance - SkinWidth) * dir;
                _velocityY = 0f;
                break;
            }
        }
        return delta;
    }

    private Vector2 ResolveHorizontal(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) < 0.0001f) return delta;

        float dir = Mathf.Sign(delta.x);
        float rayLength = Mathf.Abs(delta.x) + SkinWidth;
        float halfH = _bc.size.y * 0.5f - SkinWidth;
        Vector2 center = (Vector2)transform.position + _bc.offset;

        for (int i = 0; i < RayCount; i++)
        {
            float t = (RayCount == 1) ? 0.5f : (float)i / (RayCount - 1);
            Vector2 origin = center + Vector2.up * Mathf.Lerp(-halfH, halfH, t);
            origin.x += dir * (_bc.size.x * 0.5f - SkinWidth);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, rayLength, solidLayer);
            if (hit.collider != null)
            {
                delta.x = (hit.distance - SkinWidth) * dir;
                _velocityX = 0f;
                break;
            }
        }
        return delta;
    }
}
