using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : IManager
{
    private bool _init = false;

    public GameObject CurrentMap {  get; private set; }
    public MapState CurrentMapState { get; private set; }
    public MapDataSO CurrentMapData { get; private set;}

    private Dictionary<int, GameObject> _mapDict = new Dictionary<int, GameObject>();

    public void Init()
    {
        if (_init) return;
        _init = true;

        GameObject mapRoot = GameObject.Find("@MapRoot");
        if (mapRoot == null)
        {
            mapRoot = new GameObject("@MapRoot");
        }

        // 에디터에서 맵 오브젝트를 꺼놔도 인식
        MapController[] allMapsInScene = Object.FindObjectsByType<MapController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (MapController mapCtrl in allMapsInScene)
        {
            if (mapCtrl.mapData == null)
            {
                Debug.LogError($"[MapManager] {mapCtrl.gameObject.name}에 MapDataSO가 할당되지 않았습니다!");
                continue;
            }

            GameObject mapObj = mapCtrl.gameObject;

            if (mapObj.transform.parent != mapRoot.transform)
            {
                mapObj.transform.SetParent(mapRoot.transform, false);
            }

            RegisterMap(mapCtrl.mapData.mapID, mapObj);
            mapObj.SetActive(false);
        }
    }

    /// <summary>
    /// 씬에 배치된 맵을 딕셔너리에 등록
    /// </summary>
    public void RegisterMap(int mapID, GameObject mapObj)
    {
        if (!_mapDict.ContainsKey(mapID))
        {
            _mapDict.Add(mapID, mapObj);
        }
    }

    /// <summary>
    /// 맵 이동을 요청할때 호출하는 함수
    /// </summary>
    public void TransitionMap(MapState nextMapState, MapDataSO nextMapData, float duration = 1.0f)
    {
        if (nextMapData == null)
        {
            Debug.LogError("[MapManager] 다음 맵 데이터가 없어서 맵 이동에 실패했습니다");
            return;
        }
        Managers.Instance.StartCoroutine(CoTransitionMap(nextMapState, nextMapData, duration));
    }

    public IEnumerator CoTransitionMap(MapState nextMapState, MapDataSO nextMapData, float duration = 1.0f)
    {
        // 이동 준비 (입력 차단, ...
        Managers.Input.SetInput(false);

        // 페이드 UI 띄우기
        UI_Popup_Fade _fade = Managers.UI.ShowPopupUI<UI_Popup_Fade>("UI_Popup_Fade");
        bool isFadeCompleted = false;

        // 페이드 아웃 시작
        _fade.FadeOut(duration, () => isFadeCompleted = true);
        yield return new WaitUntil(() => isFadeCompleted);

        // 기존 맵 끄기
        if (CurrentMap != null)
        {
            CurrentMap.SetActive(false);
        }

        CurrentMapData = nextMapData;
        CurrentMapState = nextMapState;

        // 다음 맵 켜기
        if (_mapDict.TryGetValue(CurrentMapData.mapID, out GameObject nextMap))
        {
            CurrentMap = nextMap;
            RestoreMapState(CurrentMapData); // 현재 맵 동적 상태 복구
            CurrentMap.SetActive(true);
        }
        else
        {
            Debug.LogError($"[MapManager] 씬에 ID가 {CurrentMapData.mapID}인 맵이 배치되지 않았습니다!");
        }

        // 페이드 인 시작
        isFadeCompleted = false;
        _fade.FadeIn(duration, () => isFadeCompleted = true);
        yield return new WaitUntil(() => isFadeCompleted);

        // 맵 이동 완료
        Managers.UI.CloseAllPopupUI();
        Managers.Input.SetInput(true);
    }

    private void RestoreMapState(MapDataSO mapDataSO)
    {
        // BGM 세팅
        if (mapDataSO.bgm != null)
        {
            // Managers.Sound.PlayBGM(mapdataSO.bgm);
        }

        // 아이템 배치 상태 세팅
        foreach (var itemData in mapDataSO.itemSpawns)
        {
            if (CurrentMapState.lootedItemUniqueIds.Contains(itemData.uniqueID)) continue; // 이미 주운 아이템은 생성하지 않음
            // TODO: itemSpawnData를 보고 아이템 배치
        }

        // 상호작용 상태 세팅
        foreach (var interactData in mapDataSO.interactableSpawns)
        {
            if (CurrentMapState.interactedObjectUniqueIds.Contains(interactData.uniqueID)) continue; // 상호작용 상태 복원
            // TODO: interactableSpawnDatas를 보고 상호작용 여부 결정
        }

        // 다른 초기 세팅들..
        // TODO: 플레이어 시작 위치, 카메라 위치 등등..
    }

    public void Clear()
    {
        _mapDict.Clear();
        CurrentMapData = null;
        CurrentMapState = null;
        CurrentMap = null;
    }

    public void OnDestroy()
    {
        _init = false;
    }
}
