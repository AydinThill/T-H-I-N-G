using UnityEngine;

public class HandPositionInBoxNormalizedPosition : MonoBehaviour
{
    [Header("Output: Raw Local Position (X,Y,Z)")]
    public Vector3 handPosLocal;

    [Header("Output: Normalized Position (0–1 on each axis)")]
    public Vector3 handPosNormalized;

    private Transform handInside;
    private BoxCollider box;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box == null)
            Debug.LogError("This script needs to be on an object with a BoxCollider.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            handInside = other.transform;
            Debug.Log("Hand entered the box.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == handInside)
        {
            Debug.Log("Hand exited the box.");
            handInside = null;
            handPosLocal = Vector3.zero;
            handPosNormalized = Vector3.zero;
        }
    }

    private void Update()
    {
        if (handInside != null)
        {
            // Raw local coordinate
            Vector3 localPos = transform.InverseTransformPoint(handInside.position);
            handPosLocal = localPos;

            // Normalized coordinate
            Vector3 size = box.size;

            float nx = Mathf.InverseLerp(-size.x / 2f, size.x / 2f, localPos.x);
            float ny = Mathf.InverseLerp(-size.y / 2f, size.y / 2f, localPos.y);
            float nz = Mathf.InverseLerp(-size.z / 2f, size.z / 2f, localPos.z);

            handPosNormalized = new Vector3(nx, ny, nz);

            Debug.Log(
                $"Local XYZ = {localPos},   Normalized XYZ = {handPosNormalized}"
            );
        }
    }
}
