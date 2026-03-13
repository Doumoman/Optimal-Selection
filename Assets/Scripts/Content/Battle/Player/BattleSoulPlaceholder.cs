using UnityEngine;

public class BattleSoulPlaceholder : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 minBounds = new Vector2(-2f, -1f);
    [SerializeField] private Vector2 maxBounds = new Vector2(2f, 1f);

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, y, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += move;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }
}