using UnityEngine;

public class HandPositionInBox : MonoBehaviour
{
    [Header("Output: Hand Position Inside Box (Local X,Y,Z)")]
    public Vector3 handPosXYZ;

    private Transform handInside;

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
            handPosXYZ = Vector3.zero;
        }
    }

    private void Update()
    {
        if (handInside != null)
        {
            // convert world position → local box position
            Vector3 localPos = transform.InverseTransformPoint(handInside.position);

            // store X, Y, Z
            handPosXYZ = localPos;

            // log to console
            Debug.Log($"Hand Position in Box: X = {localPos.x:F3}, Y = {localPos.y:F3}, Z = {localPos.z:F3}");
        }
    }
}
