using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : IManager
{
    private bool _init = false;
    private string _savePath;

    // 인게임에서 실시간으로 읽고 쓸 데이터 원본
    public GameData CurrentData { get; private set; }

    public static event Action<int> OnGoldChanged;
    public static event Action<int> OnHealthChanged;
    public static event Action<int> OnMaliceChanged;
    public static event Action<int> OnLevelChanged;

    public void Init()
    {
        if (_init) return;
        _init = true;

        // 세이브 파일이 저장될 절대 경로 설정
        _savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
        LoadGame();
    }

    // 데이터 저장
    public void SaveGame()
    {
        // GameData 객체를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(CurrentData, true);

        // 지정된 경로에 파일 쓰기
        // 현재는 로컬 저장 방식, 추후에 변경 가능
        File.WriteAllText(_savePath, json);
        Debug.Log($"[DataManager] 데이터 저장 완료: {_savePath}");
    }

    // 데이터 불러오기
    public void LoadGame()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                CurrentData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"[DataManager] 데이터 로드 완료: {_savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 세이브 파일 로드 실패. 새 데이터를 생성합니다. Error: {e.Message}");
                CurrentData = new GameData();
            }
        }
        else
        {
            CurrentData = new GameData();
            Debug.Log("[DataManager] 세이브 파일이 없습니다. 새 게임 데이터를 생성했습니다.");
        }
    }

    #region 데이터 헬퍼 함수 (Player Data)

    // 골드 획득 및 사용
    public void UpdateGold(int amount)
    {
        CurrentData.playerGold += amount;
        if (CurrentData.playerGold < 0) CurrentData.playerGold = 0;
        OnGoldChanged?.Invoke(CurrentData.playerGold);
    }

    // 체력 증감
    public void UpdateHealth(int amount)
    {
        CurrentData.playerHealth += amount;
        if (CurrentData.playerHealth < 0) CurrentData.playerHealth = 0;
        OnHealthChanged?.Invoke(CurrentData.playerHealth);
    }

    // 악의 증감
    public void UpdateMalice(int amount)
    {
        CurrentData.playerMalice += amount;
        OnMaliceChanged?.Invoke(CurrentData.playerMalice);
    }

    // 레벨 업
    public void UpdateLevel(int level)
    {
        CurrentData.playerLevel = level;
        if (CurrentData.playerHealth < 0) CurrentData.playerLevel = 1;
        OnLevelChanged?.Invoke(CurrentData.playerLevel);
        // 레벨업 -> 최대 HP, 악의 등등 조정?
    }

    #endregion

    #region 데이터 헬퍼 함수 (Inventory)

    // 아이템 획득 로직
    public void AddInventoryItem(int itemId, string itemStringId, int count)
    {

    }

    // 아이템 사용/버리기 로직 (true 반환 시 성공적으로 소모됨)
    public bool RemoveInventoryItem(int itemId, string itemStringId, int amount)
    {
        return true;
    }

    #endregion

    #region 데이터 헬퍼 함수 (Events & World State)

    public void UpdateMonsterKill()
    {
        CurrentData.monstersKilled++;
    }

    public void UpdateClearChapter(int chapterNum)
    {
        if (!CurrentData.clearedChapters.Contains(chapterNum))
            CurrentData.clearedChapters.Add(chapterNum);
        // 특정 이벤트 발동 조건을 체크할 때 사용 (ex. 불살 루트, 몰살 루트..)
    }

    public void UpdateKillMainNPC(string npcId)
    {
        if (!CurrentData.killedMainNPCIds.Contains(npcId))
            CurrentData.killedMainNPCIds.Add(npcId);
        // 특정 이벤트 발동 조건을 체크할 때 사용 (ex. 몰살 루트..)
    }

    public void UpdateInteractedObject(string objectId)
    {
        if (!CurrentData.interactedObjectIDs.Contains(objectId))
            CurrentData.interactedObjectIDs.Add(objectId);
        // 맵 로드 될 때 오브젝트 상호작용 여부 결정 (ex. 이미 열었던 문은 계속 열려있음..)
    }

    public void UpdateLootedItem(string itemId)
    {
        if (!CurrentData.lootedItemIDs.Contains(itemId))
            CurrentData.lootedItemIDs.Add(itemId);
        // 드랍되어 있는 아이템 획득 했을 때
    }

    public void UpdateUnlockMap(string mapId)
    {
        if (!CurrentData.unlockedMapIds.Contains(mapId))
            CurrentData.unlockedMapIds.Add(mapId);
        // 맵 창? 열었을 때 해금된 맵 업데이트
    }

    #endregion

    public void Clear()
    {
        // 필요 시 메모리에 있는 데이터 초기화 로직 작성
    }

    public void OnDestroy()
    {
        // 게임이 꺼지거나 매니저가 파괴될 때 안전하게 마지막 상태 자동 저장
        if (CurrentData != null)
        {
            SaveGame();
        }
        _init = false;
    }
}