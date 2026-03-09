using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    private Vector2 _direction;
    private float _speed;
    private float _timer;

    public void Init(Vector2 direction, float speed)
    {
        _direction = direction.normalized;
        _speed = speed;
        _timer = 0f;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[BulletBase] 플레이어 피격");

            // TODO:
            // 플레이어 데미지 처리
            Destroy(gameObject);
        }
    }
}