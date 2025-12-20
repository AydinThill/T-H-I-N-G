using UnityEngine;

public class HandVelocityTracker : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }

    Vector3 _lastPos;

    void OnEnable()
    {
        _lastPos = transform.position;
        Velocity = Vector3.zero;
    }

    void Update()
    {
        var pos = transform.position;

        if (Time.deltaTime > 0f)
            Velocity = (pos - _lastPos) / Time.deltaTime;
        else
            Velocity = Vector3.zero;

        _lastPos = pos;
    }
}
