using UnityEngine;

[System.Serializable]
public class ItemSpawnData
{
    public int uniqueID; // 맵 내에서 아이템을 식별하기 위한 고유 배치 ID
    public ItemDataSO itemData; // 아이템 데이터 원본
    public int count = 1;
    public Vector2 spawnPosition; // 맵 내 스폰 위치(로컬)
}