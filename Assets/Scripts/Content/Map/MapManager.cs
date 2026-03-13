using System.Collections;
using UnityEngine;

public class MapManager : IManager
{
    private bool _init = false;

    public GameObject CurrentMap {  get; private set; }
    public MapState CurrentMapState { get; private set; }
    public MapDataSO CurrentMapData { get; private set;}

    public void Init()
    {
        if(_init) return;
        _init = true;

    }

    /// <summary>
    /// 맵 이동을 요청할때 호출하는 함수
    /// </summary>
    public void TransitionMap(MapState nextMapState, MapDataSO nextMapData, float duration = 1.0f)
    {
        if(nextMapData == null)
        {
            Debug.LogError("[MapManager] 다음 맵 데이터가 없어서 맵 이동에 실패했습니다");
            return;
        }
        Managers.Instance.StartCoroutine(CoTransitionMap(nextMapState, nextMapData, duration));
    }

    public IEnumerator CoTransitionMap(MapState nextMapState, MapDataSO nextMapData, float duration = 1.0f)
    {
        // 1. 이동 준비 (입력 차단, ...
        Managers.Input.SetInput(false);

        // 2. 페이드 UI 띄우기
        UI_Popup_Fade _fade = Managers.UI.ShowPopupUI<UI_Popup_Fade>("UI_Popup_Fade");
        bool isFadeCompleted = false;

        // 3. 페이드 아웃 시작
        _fade.FadeOut(duration, () => isFadeCompleted = true);
        yield return new WaitUntil(() => isFadeCompleted);

        // 4. 기존 맵 상태 저장
        SaveCurrentMapState();

        // 5. 기존 맵 파괴
        DestroyMap();
        CurrentMapState = nextMapState;

        // 6 & 7. 새로운 맵 초기 생성, 맵 동적 상태 복구
        // TODO
        LoadMap(nextMapData);
  

        // 8. 페이드 인 시작
        isFadeCompleted = false;
        _fade.FadeIn(duration, () => isFadeCompleted = true);
        yield return new WaitUntil(() => isFadeCompleted);

        // 9. 맵 이동 완료
        Managers.UI.CloseAllPopupUI();
        Managers.Input.SetInput(true);
    }

    private void LoadMap(MapDataSO mapDataSO)
    {
        CurrentMapData = mapDataSO;

        // 맵 프리팹 생성
        if(mapDataSO.mapPrefab != null)
        {
            CurrentMap = UnityEngine.Object.Instantiate(mapDataSO.mapPrefab);
            CurrentMap.name = $"@Map_{mapDataSO.mapStringID}";
        }
        else
        {
            Debug.LogError($"[MapManager] MapDataSO ({mapDataSO.mapName}) does not have a mapPrefab assigned!");
            return;
        }

        // BGM 세팅
        if (mapDataSO.bgm != null)
        {
            // Managers.Sound.PlayBGM(mapdataSO.bgm);
        }

        // 다른 초기 세팅들..
        // TODO

        foreach(var itemData in mapDataSO.itemSpawns)
        {
            if (CurrentMapState.lootedItemUniqueIds.Contains(itemData.uniqueID)) continue; // 이미 주운 아이템은 생성하지 않음
            // TODO: itemSpawnData를 보고 아이템 배치
        }

        foreach (var interactData in mapDataSO.interactableSpawns)
        {


            if (CurrentMapState.interactedObjectUniqueIds.Contains(interactData.uniqueID)) continue; // 상호작용 상태 복원
            // TODO:
        }

    }

    private void SaveCurrentMapState()
    {

    }

    private void DestroyMap()
    {
        if (CurrentMap != null)
        {
            UnityEngine.Object.Destroy(CurrentMap);
            CurrentMap = null;
        }

        CurrentMapData = null;
    }

    public void Clear()
    {
        DestroyMap();
    }

    public void OnDestroy()
    {
        DestroyMap();
        _init = false;
    }
}
