using UnityEngine;

public class BattleFieldView : MonoBehaviour
{
    [Header("Scene Refs")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject background;
    [SerializeField] private Transform enemyAnchor;
    [SerializeField] private Transform soulSpawn;
    [SerializeField] private Transform bulletRoot;

    [Header("Default Prefabs")]
    [SerializeField] private GameObject defaultEnemyPrefab;
    [SerializeField] private GameObject defaultSoulPrefab;

    private GameObject _currentEnemyInstance;
    private GameObject _currentSoulInstance;

    public Transform EnemyAnchor => enemyAnchor;
    public Transform SoulSpawn => soulSpawn;
    public Transform BulletRoot => bulletRoot;

    public void Show()
    {
        if (root != null)
            root.SetActive(true);

        if (background != null)
            background.SetActive(true);
    }

    public void Hide()
    {
        ClearSpawnedObjects();

        if (background != null)
            background.SetActive(false);

        if (root != null)
            root.SetActive(false);
    }

    public void SpawnEnemy(GameObject enemyPrefab = null)
    {
        GameObject prefabToUse = enemyPrefab != null ? enemyPrefab : defaultEnemyPrefab;

        if (prefabToUse == null || enemyAnchor == null)
            return;

        ClearEnemy();

        _currentEnemyInstance = Instantiate(prefabToUse, enemyAnchor.position, Quaternion.identity, enemyAnchor);
    }

    public void SpawnSoul(GameObject soulPrefab = null)
    {
        GameObject prefabToUse = soulPrefab != null ? soulPrefab : defaultSoulPrefab;

        if (prefabToUse == null || soulSpawn == null)
            return;

        ClearSoul();

        _currentSoulInstance = Instantiate(prefabToUse, soulSpawn.position, Quaternion.identity, soulSpawn);
    }

    public void ClearEnemy()
    {
        if (_currentEnemyInstance != null)
        {
            Destroy(_currentEnemyInstance);
            _currentEnemyInstance = null;
        }
    }

    public void ClearSoul()
    {
        if (_currentSoulInstance != null)
        {
            Destroy(_currentSoulInstance);
            _currentSoulInstance = null;
        }
    }

    public void ClearBullets()
    {
        if (bulletRoot == null) return;

        for (int i = bulletRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(bulletRoot.GetChild(i).gameObject);
        }
    }

    public void ClearSpawnedObjects()
    {
        ClearEnemy();
        ClearSoul();
        ClearBullets();
    }
}