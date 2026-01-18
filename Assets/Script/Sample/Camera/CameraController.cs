using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float lerpSpeed = 12f;

    private Transform targetTransform;

    private void Start()
    {
        targetTransform = target.GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = targetTransform.position + offset;
        Vector3 lerpPos = Vector3.Lerp(transform.position, targetPos, lerpSpeed);
        transform.position = lerpPos;
    }
}
