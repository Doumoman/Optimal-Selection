// RingManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingManager : MonoBehaviour
{
    [Header("Refs")]
    public Transform center;
    public RingSpawner spawner;

    [Tooltip("링들이 달릴 부모(ScaleUp 대상). 비우면 this.transform")]
    public Transform ringRoot;

    [Header("Keep Rings")]
    public int keepRingCount = 5;
    public float outerRadius = 3.5f;
    public float radiusSpan = 4f;
    public float[] gapRatios = { 7f, 6f, 5f, 4f };

    [Header("Thickness Perspective")]
    public float outerThickness = 0.35f;
    public float innerThicknessScale = 0.6f;

    [Header("Zoom Feel")]
    public float zoomScale = 1.25f;
    public float zoomDuration = 0.45f; // 끊김 방지: 좀 길게

    public float[] radii { get; private set; }
    public List<RingInstance> spawnedRings { get; private set; } = new();

    private bool _zooming;
    public bool IsZooming => _zooming;

    void Awake()
    {
        if (!ringRoot) ringRoot = transform;
        BuildInitial();
    }

    public void BuildInitial()
    {
        for (int i = ringRoot.childCount - 1; i >= 0; i--)
            Destroy(ringRoot.GetChild(i).gameObject);

        spawnedRings.Clear();

        radii = ComputeRadii(keepRingCount, outerRadius, radiusSpan, gapRatios);

        if (spawner) spawner.center = center;

        for (int i = 0; i < keepRingCount; i++)
        {
            float t = (keepRingCount <= 1) ? 0f : (i / (keepRingCount - 1f));
            float thick = Mathf.Lerp(outerThickness, outerThickness * innerThicknessScale, t);

            var inst = spawner.SpawnRing(ringRoot, radii[i], thick);
            spawnedRings.Add(inst);
        }
    }

    public void RequestDescend()
    {
        if (_zooming) return;
        StartCoroutine(CoDescend());
    }

    // 끊김 줄이기: 스케일 흐름은 계속, 중간에 쉬프트만 끼워넣기
    private IEnumerator CoDescend()
    {
        _zooming = true;

        Vector3 baseScale = ringRoot.localScale;
        Vector3 peakScale = baseScale * zoomScale;

        float total = Mathf.Max(0.01f, zoomDuration);
        bool shifted = false;

        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            float u = t / total;               // 0..1
            float eased = EaseInOutCubic(u);   // 0..1

            // 0->1->0 웨이브(연속)
            float wave = 1f - Mathf.Abs(2f * eased - 1f);

            ringRoot.localScale = Vector3.Lerp(baseScale, peakScale, wave);

            if (!shifted && u >= 0.50f)
            {
                ShiftRingsOnce();
                shifted = true;
            }

            yield return null;
        }

        ringRoot.localScale = baseScale;
        _zooming = false;
    }

    private void ShiftRingsOnce()
    {
        if (spawnedRings.Count == 0) return;

        // 1) 바깥 링 제거
        var outer = spawnedRings[0];
        spawnedRings.RemoveAt(0);
        if (outer) Destroy(outer.gameObject);

        // 2) 남은 링들을 한 칸씩 바깥 반지름으로 이동 + 폭도 재계산(핵심)
        for (int i = 0; i < spawnedRings.Count; i++)
        {
            var ring = spawnedRings[i];
            ring.radius = radii[i];
            spawner.ApplyGeometry(ring);
        }

        // 3) 새 안쪽 링 추가
        float newInnerR = radii[keepRingCount - 1];
        float thick = outerThickness * innerThicknessScale;

        var newRing = spawner.SpawnRing(ringRoot, newInnerR, thick);
        spawnedRings.Add(newRing);
    }

    private static float EaseInOutCubic(float x)
    {
        return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }

    private static float[] ComputeRadii(int count, float outerR, float span, float[] ratios)
    {
        float[] result = new float[count];
        if (count <= 0) return result;

        result[0] = outerR;
        if (count == 1) return result;

        int gapCount = count - 1;

        float[] use = ratios;
        if (use == null || use.Length != gapCount)
        {
            use = new float[gapCount];
            for (int i = 0; i < gapCount; i++) use[i] = 1f;
        }

        float sum = 0f;
        for (int i = 0; i < gapCount; i++) sum += use[i];
        float baseGap = span / Mathf.Max(0.0001f, sum);

        float r = outerR;
        for (int i = 0; i < count; i++)
        {
            result[i] = r;
            if (i < gapCount) r -= baseGap * use[i];
        }
        return result;
    }
}
