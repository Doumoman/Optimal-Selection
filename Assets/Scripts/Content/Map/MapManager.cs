using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : IManager
{
    private bool _init = false;

    public GameObject CurrentMap { get; private set; }
    public MapState CurrentMapState { get; private set; }
    public MapDataSO CurrentMapData { get; private set; }

    private Dictionary<int, GameObject> _mapDict = new Dictionary<int, GameObject>();

    private ChapterSceneRefs _sceneRefs;

    public void Init()
    {
        if (_init) return;
        _init = true;

        _mapDict.Clear();
    }

    public void RegisterScene(ChapterSceneRefs refs)
    {
        _sceneRefs = refs;
        _mapDict.Clear();

        if (_sceneRefs.mapRoot == null) return;

        // 에디터에서 맵을 꺼놔서 인식
        MapController[] allMaps = _sceneRefs.mapRoot.GetComponentsInChildren<MapController>(true);

        foreach (var map in allMaps)
        {
            if (map.mapData == null) continue;
            _mapDict[map.mapData.mapID] = map.gameObject;
            map.gameObject.SetActive(false);
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
    public void TransitionMap(MapState nextMapState, MapDataSO nextMapData,
    MapPortal.PortalDirection entryDirection = MapPortal.PortalDirection.None, float duration = 1.0f)
    {
        if (nextMapData == null)
        {
            Debug.LogError("[MapManager] 다음 맵 데이터가 없어서 맵 이동에 실패했습니다");
            return;
        }
        SingletonManagers.Instance.StartCoroutine(CoTransitionMap(nextMapState, nextMapData, entryDirection, duration));
    }

    public IEnumerator CoTransitionMap(MapState nextMapState, MapDataSO nextMapData,
    MapPortal.PortalDirection entryDirection = MapPortal.PortalDirection.None, float duration = 1.0f)
    {
        // 이동 준비 (입력 차단, ...
        SingletonManagers.Input.SetInput(false);

        // 페이드 UI 띄우기
        UI_Popup_Fade _fade = SingletonManagers.UI.GetFade();
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
 
            TeleportPlayerToPortal(entryDirection); // 플레이어 순간이동
            SnapCameraToPlayer(_sceneRefs.playerFSM.gameObject); // 카메라 순간이동
        }
        else
        {
            Debug.LogError($"[MapManager] 씬에 ID가 {CurrentMapData.mapID}인 맵이 배치되지 않았습니다!");
        }

        yield return null;

        // 페이드 인 시작
        isFadeCompleted = false;
        _fade.FadeIn(duration, () => isFadeCompleted = true);
        yield return new WaitUntil(() => isFadeCompleted);

        // 맵 이동 완료
        SingletonManagers.UI.CloseAllPopupUI();
        SingletonManagers.Input.SetInput(true);
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


    /// <summary>
    /// 플레이어를 포탈 앞으로 텔레포트 시킴
    /// </summary>
    public void TeleportPlayerToPortal(MapPortal.PortalDirection entryDirection)
    {
        PlayerFSM player = _sceneRefs.playerFSM;
        if (player == null)
        {
            Debug.LogWarning("[MapManager] 씬에 Player 태그를 가진 오브젝트가 없습니다!");
            return;
        }

        // 포탈을 통한 이동이 아님, 세이브/로드 
        if (entryDirection == MapPortal.PortalDirection.None)
        {
            player.transform.position = SingletonManagers.Data.CurrentData.currentMapPosition;
            return;
        }
        else
        {
            // 포탈을 통한 이동
            MapPortal[] portals = CurrentMap.GetComponentsInChildren<MapPortal>();
            foreach (var portal in portals)
            {
                if (portal.direction == entryDirection)
                {
                    Vector2 portalWorldPosition = portal.transform.position;
                    Vector2 spawnOffset = -portal.GetDirectionCoords() * 2;
                    player.transform.position = portalWorldPosition + spawnOffset;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 카메라를 플레이어 위치로 즉시 이동
    /// </summary>
    private void SnapCameraToPlayer(GameObject player)
    {
        if (_sceneRefs.mainCamera != null)
        {
            Vector3 camPos = player.transform.position;
            camPos.z = _sceneRefs.mainCamera.transform.position.z; // Z축 유지
            _sceneRefs.mainCamera.transform.position = camPos;
        }
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
