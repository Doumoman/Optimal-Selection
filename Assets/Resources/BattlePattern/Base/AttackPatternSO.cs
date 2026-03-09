using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Attack Pattern")]
public class AttackPatternSO : ScriptableObject
{
    [Header("Info")]
    public string patternId;
    public string displayName;

    [Header("Duration")]
    public float duration = 3f; // 패턴 전체 지속시간

    [Header("Spawn")]
    public GameObject bulletPrefab;
    public int spawnCountPerWave = 5; // 한 번 발사할 때 몇 발 생성할지
    public float spawnInterval = 0.2f; // 웨이브 간 간격

    [Header("Bullet")]
    public float bulletSpeed = 5f;
    public Vector2 direction = Vector2.down;

    [Header("Spawn Area")]
    public Transform[] fixedSpawnPoints; // 있으면 여기서 생성

    [Header("Fallback Random Spawn")]
    public bool useRandomSpawnX = true;
    public float spawnY = 4f;
    public float minX = -3f;
    public float maxX = 3f;
}