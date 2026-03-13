using UnityEngine;

[System.Serializable]
public class InteractableSpawnData
{
    public int uniqueID; // 고유 ID
    public GameObject prefab; // 생성 프리팹
    public Vector2 position; // 스폰 위치
}