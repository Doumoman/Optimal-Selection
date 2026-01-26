// RingSpawner.cs
using UnityEngine;

public class RingSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject segmentPrefab;  // 벽
    public GameObject holePrefab;     // 구멍(비우면 null = 진짜 빈 공간)

    [Header("Ring")]
    public Transform center;
    public int segmentCount = 60;
    public float rotationOffsetDeg = 0f;

    [Header("Gap")]
    [Range(0f, 1f)] public float gapProbability = 0.10f;
    public int forceGapIfNoGapStreak = 20;
    public int gapSizeSegments = 2;

    // 프리팹의 "로컬 기준" 크기(부모 스케일 영향 X)
    private float segBaseW, segBaseH;
    private float holeBaseW, holeBaseH;
    private bool _cached;

    public RingInstance SpawnRing(Transform parent, float radius, float thickness)
    {
        CachePrefabSizes();

        if (!parent) parent = transform;
        Vector3 c = center ? center.position : Vector3.zero;

        var ringRoot = new GameObject($"Ring_r{radius:0.00}");
        ringRoot.transform.SetParent(parent, false);
        ringRoot.transform.position = c;
        ringRoot.transform.rotation = Quaternion.identity;

        var ring = ringRoot.AddComponent<RingInstance>();
        ring.radius = radius;
        ring.segmentCount = segmentCount;
        ring.thickness = thickness;

        bool[] isGap = BuildGapMask();
        ring.gapMask = isGap;

        // 오브젝트만 생성(위치/회전/스케일은 ApplyGeometry에서 한 번에)
        for (int i = 0; i < segmentCount; i++)
        {
            bool gap = isGap[i];
            GameObject prefabToUse = gap ? holePrefab : segmentPrefab;

            // holePrefab이 null이면 빈 공간(아예 생성 X)
            if (prefabToUse == null) continue;

            GameObject seg = Instantiate(prefabToUse, ringRoot.transform);

            var meta = seg.GetComponent<SegmentMeta>();
            if (!meta) meta = seg.AddComponent<SegmentMeta>();
            meta.index = i;

            if (gap)
            {
                meta.baseW = holeBaseW;
                meta.baseH = holeBaseH;
            }
            else
            {
                meta.baseW = segBaseW;
                meta.baseH = segBaseH;
            }
        }

        ApplyGeometry(ring);
        return ring;
    }

    public void ApplyGeometry(RingInstance ring)
    {
        if (!ring) return;

        int n = ring.segmentCount;
        float r = ring.radius;
        float stepDeg = 360f / n;

        float circumference = 2f * Mathf.PI * r;
        float targetWidth = circumference / n;
        float targetHeight = ring.thickness;

        for (int ci = 0; ci < ring.transform.childCount; ci++)
        {
            Transform seg = ring.transform.GetChild(ci);
            var meta = seg.GetComponent<SegmentMeta>();
            int idx = meta ? meta.index : ci;

            float ang = idx * stepDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Vector2 tangent = new Vector2(-dir.y, dir.x);

            seg.localPosition = (Vector3)(dir * r);

            float rotZ = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg + rotationOffsetDeg;
            seg.localRotation = Quaternion.Euler(0, 0, rotZ);

            // ★ 절대 월드 bounds 사용 금지. meta.baseW/H(프리팹 로컬 기준)로만 계산
            float bw = meta ? meta.baseW : segBaseW;
            float bh = meta ? meta.baseH : segBaseH;

            if (bw > 0.0001f && bh > 0.0001f)
            {
                seg.localScale = new Vector3(targetWidth / bw, targetHeight / bh, 1f);
            }
        }
    }

    private void CachePrefabSizes()
    {
        if (_cached) return;

        (segBaseW, segBaseH) = GetLocalSpriteSize(segmentPrefab);
        (holeBaseW, holeBaseH) = GetLocalSpriteSize(holePrefab);

        // holePrefab이 null이면 값 안 써도 됨(그냥 안전)
        if (segBaseW < 0.0001f) segBaseW = 1f;
        if (segBaseH < 0.0001f) segBaseH = 1f;
        if (holeBaseW < 0.0001f) holeBaseW = segBaseW;
        if (holeBaseH < 0.0001f) holeBaseH = segBaseH;

        _cached = true;
    }

    // 프리팹 로컬 기준 크기: sprite.bounds.size (transform scale 영향 X)
    private (float w, float h) GetLocalSpriteSize(GameObject prefab)
    {
        if (!prefab) return (0f, 0f);

        var sr = prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Vector2 s = sr.sprite.bounds.size; // 로컬 단위
            return (Mathf.Abs(s.x), Mathf.Abs(s.y));
        }

        // 스프라이트 없으면 로컬스케일을 fallback
        Vector3 ls = prefab.transform.localScale;
        return (Mathf.Abs(ls.x), Mathf.Abs(ls.y));
    }

    private bool[] BuildGapMask()
    {
        bool[] isGap = new bool[segmentCount];
        int noGapStreak = 0;
        bool anyGap = false;

        for (int i = 0; i < segmentCount; i++)
        {
            bool makeGap = (Random.value < gapProbability);
            if (noGapStreak >= forceGapIfNoGapStreak) makeGap = true;

            if (makeGap)
            {
                anyGap = true;
                noGapStreak = 0;

                for (int k = 0; k < gapSizeSegments; k++)
                {
                    int idx = i + k;
                    if (idx < segmentCount) isGap[idx] = true;
                }

                i += (gapSizeSegments - 1);
            }
            else noGapStreak++;
        }

        if (!anyGap)
        {
            int start = Random.Range(0, segmentCount - gapSizeSegments);
            for (int k = 0; k < gapSizeSegments; k++)
                isGap[start + k] = true;
        }

        return isGap;
    }
}
