
using UnityEngine;

public class RingInstance : MonoBehaviour
{
    public float radius;
    public bool[] gapMask; // 60 길이 true=구멍

    [HideInInspector] public int segmentCount;
    [HideInInspector] public float thickness;
}