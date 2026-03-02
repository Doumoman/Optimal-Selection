using UnityEngine;

[System.Serializable]
public class ItemSpawnData
{
    public int ItemID; // 고유 ID
    public string ItemStringID; // 고유 문자열 ID

    public GameObject prefab; // 생성 프리팹
    public int count = 1; // 아이템 개수
    public Vector3 position; // 스폰 위치
}