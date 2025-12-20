using UnityEngine;
using FMODUnity;

public class FmodDrumHitTrigger : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference hitEvent;

    [Header("Filter")]
    public string handTag = "hand";     // must match your Tag exactly
    public float minSpeed = 0.2f;
    public float cooldown = 0.05f;

    [Header("Optional dynamics")]
    public string velocityParam = "";   // e.g. "HitVelocity"
    public float velocityParamScale = 1.0f;

    float _nextAllowedTime;

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < _nextAllowedTime) return;
        if (!other.CompareTag(handTag)) return;

        // Try to get tracked velocity from the hand
        var tracker = other.GetComponentInParent<HandVelocityTracker>();
        float speed = tracker ? tracker.Velocity.magnitude : 0f;

        if (speed < minSpeed) return;
        _nextAllowedTime = Time.time + cooldown;

        var inst = RuntimeManager.CreateInstance(hitEvent);
        inst.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        if (!string.IsNullOrEmpty(velocityParam))
            inst.setParameterByName(velocityParam, speed * velocityParamScale);

        inst.setVolume(Mathf.Clamp01(speed / 2.0f));

        inst.start();
        inst.release();
    }
}
