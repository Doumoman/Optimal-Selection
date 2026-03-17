using UnityEngine;

public class MapPortal : MonoBehaviour
{
    public enum PortalDirection
    {
        None, // 포탈로 순간이동 하지 않은 경우
        Left,
        Right,
        Up,
        Down
    }

    public PortalDirection direction; // 현재 포탈이 향하는 방향

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("플레이어 포탈 사용 가능");
        if (collision.CompareTag("Player"))
        {
            Vector2Int moveDir = GetDirectionCoords();

            var chapterManager = FindFirstObjectByType<ChapterManager>();
            if (chapterManager != null)
            {
                chapterManager.TeleportToNextMap(this);
            }
        }
    }

    public Vector2Int GetDirectionCoords()
    {
        switch (direction)
        {
            case PortalDirection.Left: return Vector2Int.left;
            case PortalDirection.Right: return Vector2Int.right;
            case PortalDirection.Up: return Vector2Int.up;
            case PortalDirection.Down: return Vector2Int.down;
            default: return Vector2Int.zero;
        }
    }

    public PortalDirection GetOppositeDirection()
    {
        switch (direction)
        {
            case PortalDirection.Left: return PortalDirection.Right;
            case PortalDirection.Right: return PortalDirection.Left;
            case PortalDirection.Up: return PortalDirection.Down;
            case PortalDirection.Down: return PortalDirection.Up;
            default: return PortalDirection.None;
        }
    }
}
