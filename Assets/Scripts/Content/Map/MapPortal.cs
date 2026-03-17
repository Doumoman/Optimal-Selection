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
                chapterManager.MoveToDirection(moveDir, direction);
            }
        }
    }

    public Vector2Int GetDirectionCoords()
    {
        // 2차원 배열로 생각하면 될듯
        switch (direction)
        {
            case PortalDirection.Left: return new Vector2Int(0, -1);
            case PortalDirection.Right: return new Vector2Int(0, 1);
            case PortalDirection.Up: return new Vector2Int(-1, 0);
            case PortalDirection.Down: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }
}
