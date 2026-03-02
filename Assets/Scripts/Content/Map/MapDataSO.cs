using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵의 초기(정적) 정보를 가지고 있음
/// </summary>
[CreateAssetMenu(fileName = "MapDataSO", menuName = "Scriptable Objects/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public int mapID;               // 맵 고유 ID
    public string mapStringID;      // 맵 고유 문자열 ID
    public string mapName;          // 맵 이름

    [Header("Visuals")]
    public GameObject mapPrefab;    // 실제 생성될 맵 프리팹

    [Header("Initial Spawns")]
    [Tooltip("바닥에 떨어져 있는 아이템 배치")]
    public List<ItemSpawnData> itemSpawns = new List<ItemSpawnData>();

    [Tooltip("상자, 레버, 문 등 상호작용 오브젝트 배치")]
    public List<ObjectSpawnData> objectSpawns = new List<ObjectSpawnData>();

    [Header("Settings")]
    public AudioClip bgm;           // 이 맵의 BGM
    public float cameraSize = 5f;   // 카메라 사이즈 (줌인, 줌아웃에 사용)
}
