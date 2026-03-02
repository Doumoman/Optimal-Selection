using UnityEngine;

[System.Serializable]
public class InteractableSpawnData
{
    public int objectID; // 고유 ID
    public string objectStringID; // 고유 문자열 ID

    public GameObject prefab; // 생성 프리팹
    public Vector3 position; // 스폰 위치
}