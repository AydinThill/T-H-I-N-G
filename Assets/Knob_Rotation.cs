using UnityEngine;

public class VRKnob : MonoBehaviour
{
    public Transform knob;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Range(0f, 1f)]
    public float knobValue;

    void Update()
    {
        float angle = knob.localEulerAngles.z;
        if (angle > 180) angle -= 360;

        knobValue = Mathf.InverseLerp(minAngle, maxAngle, angle);
    }
}
