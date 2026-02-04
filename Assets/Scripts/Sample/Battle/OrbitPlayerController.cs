// PlayerOrbitController.cs
using UnityEngine;

public class PlayerOrbitController : MonoBehaviour
{
    [Header("Refs")]
    public RingManager manager;
    public Transform center;

    [Header("Discrete Angle")]
    public int a = 0; // 0..359
    public float unitsPerSecond = 24f;

    [Header("Render")]
    public int pixelsPerUnit = 16;
    public bool smoothRender = true;
    public float renderLerp = 18f;

    [Header("Outer Follow")]
    public float radiusOffset = -0.05f;

    private float stepTimer;
    private int lastTriggeredSeg = -999;

    void Start()
    {
        if (!center && manager) center = manager.center;
        transform.position = WorldPosOnOuter(a);
    }

    void Update()
    {
        int dir = 0;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) dir = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) dir = 1;

        if (dir != 0)
        {
            stepTimer += Time.deltaTime;
            float stepInterval = 1f / Mathf.Max(1f, unitsPerSecond);

            while (stepTimer >= stepInterval)
            {
                stepTimer -= stepInterval;
                Step(dir);
            }
        }
        else stepTimer = 0f;

        Vector3 target = WorldPosOnOuter(a);
        if (smoothRender)
            transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-renderLerp * Time.deltaTime));
        else
            transform.position = target;
    }

    private void Step(int dir)
    {
        a = (a + dir + 360) % 360;

        if (manager == null || manager.IsZooming) return;
        if (manager.spawnedRings == null || manager.spawnedRings.Count == 0) return;

        int seg = a / 6; // 0..59

        var outerRing = manager.spawnedRings[0];
        if (outerRing == null || outerRing.gapMask == null) return;

        if (seg >= 0 && seg < outerRing.gapMask.Length && outerRing.gapMask[seg])
        {
            if (seg == lastTriggeredSeg) return;
            lastTriggeredSeg = seg;
            manager.RequestDescend();
        }
        else
        {
            if (seg != lastTriggeredSeg) lastTriggeredSeg = -999;
        }
    }

    private Vector3 WorldPosOnOuter(int angleIndex)
    {
        Vector2 c = center ? (Vector2)center.position : Vector2.zero;

        float baseOuterR = (manager != null && manager.radii != null && manager.radii.Length > 0)
            ? manager.radii[0]
            : 0f;

        float scale = 1f;
        if (manager != null && manager.ringRoot != null)
            scale = manager.ringRoot.lossyScale.x; // 균일 스케일 가정

        float r = (baseOuterR + radiusOffset) * scale;

        float theta = (angleIndex / 360f) * Mathf.PI * 2f;
        Vector2 pos = c + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * r;

        return SnapToPixel(pos);
    }

    private Vector3 SnapToPixel(Vector2 worldPos)
    {
        float ppu = pixelsPerUnit;
        float x = Mathf.Round(worldPos.x * ppu) / ppu;
        float y = Mathf.Round(worldPos.y * ppu) / ppu;
        return new Vector3(x, y, 0f);
    }
}
