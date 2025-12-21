using UnityEngine;

public class WheelGrabRotate : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag used by the hand collider objects.")]
    public string handTag = "Hand";

    [Header("Rotation Space (recommended: parent space)")]
    [Tooltip("Wheel axis in PARENT local space. Usually Y for a wheel.")]
    public Vector3 parentLocalAxis = Vector3.up;

    [Tooltip("Reference direction in PARENT local space used as 0° direction (must not be parallel to axis).")]
    public Vector3 parentLocalReferenceDir = Vector3.forward;

    [Header("Clamp (degrees)")]
    public bool clampAngle = true;
    public float minAngle = 0f;
    public float maxAngle = 270f;

    [Header("Behaviour")]
    public bool invert = false;

    [Tooltip("Optional: smooth the rotation a bit.")]
    public bool smoothing = false;
    public float smoothSpeed = 20f;

    private Transform grabbingHand;
    private Transform parentTf;

    private Quaternion restLocalRot;      // wheel local rotation at start
    private float currentAngleFromRest;   // degrees
    private float grabStartHandAngle;     // degrees
    private float grabStartWheelAngle;    // degrees

    private void Awake()
    {
        parentTf = transform.parent;
        if (parentTf == null)
            Debug.LogError($"{nameof(WheelGrabRotate)} expects the wheel to have a parent (e.g. MAIN_RAD_PARENT).", this);

        restLocalRot = transform.localRotation;
        currentAngleFromRest = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (grabbingHand != null) return;
        if (!other.CompareTag(handTag)) return;

        grabbingHand = other.transform;

        // Cache the starting angles at the moment you "grab"
        grabStartHandAngle = GetHandAngleDeg(grabbingHand.position);
        grabStartWheelAngle = currentAngleFromRest;
    }

    private void OnTriggerExit(Collider other)
    {
        if (grabbingHand == null) return;
        if (other.transform != grabbingHand) return;

        grabbingHand = null;
    }

    private void Update()
    {
        if (grabbingHand == null || parentTf == null) return;

        float handAngle = GetHandAngleDeg(grabbingHand.position);
        float delta = Mathf.DeltaAngle(grabStartHandAngle, handAngle);

        if (invert) delta = -delta;

        float targetAngle = grabStartWheelAngle + delta;

        if (clampAngle)
            targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

        if (smoothing)
        {
            currentAngleFromRest = Mathf.Lerp(
                currentAngleFromRest,
                targetAngle,
                1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)
            );
        }
        else
        {
            currentAngleFromRest = targetAngle;
        }

        // Apply in wheel local space: rest rotation + angle around parent-local axis
        Vector3 axisLocal = parentLocalAxis.normalized;
        Quaternion rotOffsetInParent = Quaternion.AngleAxis(currentAngleFromRest, axisLocal);

        // localRotation is relative to parent
        transform.localRotation = restLocalRot * rotOffsetInParent;
    }

    /// <summary>
    /// Returns hand angle around the wheel axis, in degrees, measured in PARENT LOCAL SPACE.
    /// </summary>
    private float GetHandAngleDeg(Vector3 handWorldPos)
    {
        // Convert positions to parent local space to avoid feedback as the wheel rotates
        Vector3 wheelCentreLocal = parentTf.InverseTransformPoint(transform.position);
        Vector3 handLocal = parentTf.InverseTransformPoint(handWorldPos);

        Vector3 axis = parentLocalAxis.normalized;

        Vector3 v = handLocal - wheelCentreLocal;

        // Project v onto plane orthogonal to axis
        v -= axis * Vector3.Dot(v, axis);

        if (v.sqrMagnitude < 1e-6f)
            return 0f;

        // Build a stable 2D basis in that plane
        Vector3 refDir = parentLocalReferenceDir;

        // Ensure reference dir isn't parallel to axis
        if (Mathf.Abs(Vector3.Dot(refDir.normalized, axis)) > 0.99f)
            refDir = Vector3.right;

        // Make refDir perpendicular to axis
        refDir -= axis * Vector3.Dot(refDir, axis);
        refDir = refDir.normalized;

        Vector3 refDir90 = Vector3.Cross(axis, refDir).normalized;

        // Coordinates in the plane basis
        float x = Vector3.Dot(v, refDir);
        float y = Vector3.Dot(v, refDir90);

        return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }
}
