using System;
using System.Reflection;
using UnityEngine;

public class RadialBpmWheel : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform mainRad;                // MAIN_RAD (wheel you rotate)
    [SerializeField] private Transform animationGroup;         // MAIN_RAD_PARENT/ANIMATION (parent of tubes)
    [SerializeField] private MonoBehaviour sequencerComponent; // Sequencer script component

    [Header("Sequencer BPM Binding")]
    [SerializeField] private string bpmMemberName = "Bpm";     // field/property name (case-insensitive fallback)
    [SerializeField] private bool bpmIsInt = false;            // tick if Sequencer.Bpm is int
    [SerializeField] private bool logBindingDebug = true;      // prints what it finds / why it fails

    [Header("Wheel Axis & Range")]
    [Tooltip("Axis in MAIN_RAD local space that represents the wheel rotation. Z axis = (0,0,1).")]
    [SerializeField] private Vector3 localAxis = Vector3.forward; // Z axis
    [Tooltip("A direction perpendicular to the axis, used to compute signed angle reliably.")]
    [SerializeField] private Vector3 localReferenceDir = Vector3.up;

    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 270f;

    [Header("BPM Range")]
    [SerializeField] private float minBpm = 60f;
    [SerializeField] private float maxBpm = 180f;

    [Header("Animation Tubes")]
    [Tooltip("If empty, we auto-collect all direct children under ANIMATION.")]
    [SerializeField] private Transform[] tubes;

    [Tooltip("Each tube will rotate around its OWN local axis (same axis vector as 'localAxis').")]
    [SerializeField] private bool rotateTubesAroundOwnAxis = true;

    [Header("Smoothing (optional)")]
    [SerializeField] private bool smoothBpm = true;
    [SerializeField] private float bpmLerpSpeed = 12f;

    // start pose
    private Quaternion mainRadStartLocalRot;
    private Quaternion[] tubeStartLocalRots;

    // reflection cache
    private FieldInfo bpmField;
    private PropertyInfo bpmProp;

    private float currentBpm;

    private void Awake()
    {
        if (mainRad == null)
            Debug.LogError($"{nameof(RadialBpmWheel)}: mainRad not set.", this);

        if (animationGroup == null)
            Debug.LogError($"{nameof(RadialBpmWheel)}: animationGroup not set.", this);

        if (sequencerComponent == null)
            Debug.LogError($"{nameof(RadialBpmWheel)}: sequencerComponent not set.", this);

        if (mainRad != null) mainRadStartLocalRot = mainRad.localRotation;

        // tubes: direct children of ANIMATION
        if ((tubes == null || tubes.Length == 0) && animationGroup != null)
            tubes = GetAllChildren(animationGroup);

        tubeStartLocalRots = new Quaternion[tubes.Length];
        for (int i = 0; i < tubes.Length; i++)
            tubeStartLocalRots[i] = tubes[i] != null ? tubes[i].localRotation : Quaternion.identity;

        CacheBpmMember();

        currentBpm = Mathf.Clamp(minBpm, minBpm, maxBpm);
        SetSequencerBpm(currentBpm, force: true);
    }

    private void Update()
    {
        if (mainRad == null) return;

        float angleDelta = GetSignedAngleDeltaFromStart(
            mainRadStartLocalRot,
            mainRad.localRotation,
            localAxis,
            localReferenceDir
        );

        float clampedAngle = Mathf.Clamp(angleDelta, minAngle, maxAngle);
        float t = Mathf.InverseLerp(minAngle, maxAngle, clampedAngle);
        float targetBpm = Mathf.Lerp(minBpm, maxBpm, t);

        if (smoothBpm)
            currentBpm = Mathf.Lerp(currentBpm, targetBpm, 1f - Mathf.Exp(-bpmLerpSpeed * Time.deltaTime));
        else
            currentBpm = targetBpm;

        SetSequencerBpm(currentBpm, force: false);

        // IMPORTANT: do NOT rotate the animationGroup object itself.
        // Instead rotate each tube relative to its own stored start rotation.
        ApplyRotationOffsetToTubes(clampedAngle);
    }

    private void ApplyRotationOffsetToTubes(float angleDegrees)
    {
        if (tubes == null || tubeStartLocalRots == null) return;

        Vector3 axis = localAxis.normalized;
        Quaternion offset = Quaternion.AngleAxis(angleDegrees, axis);

        for (int i = 0; i < tubes.Length; i++)
        {
            Transform tube = tubes[i];
            if (tube == null) continue;

            if (rotateTubesAroundOwnAxis)
            {
                // Add/subtract wheel rotation to each tube's ORIGINAL local rotation
                // => tubeStart + offset
                tube.localRotation = tubeStartLocalRots[i] * offset;
            }
            else
            {
                // If you ever need "global group rotation" behaviour again:
                tube.localRotation = offset * tubeStartLocalRots[i];
            }
        }
    }

    /// <summary>
    /// Stable signed angle between start and current around a local axis, using a reference direction.
    /// </summary>
    private static float GetSignedAngleDeltaFromStart(
        Quaternion start,
        Quaternion current,
        Vector3 localAxis,
        Vector3 localRefDir
    )
    {
        Quaternion delta = Quaternion.Inverse(start) * current;

        Vector3 axis = localAxis.normalized;

        Vector3 refDir = localRefDir;
        if (refDir == Vector3.zero) refDir = Vector3.up;

        // Ensure refDir isn't parallel to axis
        if (Mathf.Abs(Vector3.Dot(refDir.normalized, axis)) > 0.99f)
            refDir = Vector3.right;

        refDir = refDir.normalized;

        Vector3 curDir = (delta * refDir).normalized;
        return Vector3.SignedAngle(refDir, curDir, axis);
    }

    private void CacheBpmMember()
    {
        if (sequencerComponent == null) return;

        Type t = sequencerComponent.GetType();
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        // Exact
        bpmField = t.GetField(bpmMemberName, flags);
        bpmProp = t.GetProperty(bpmMemberName, flags);

        // Case-insensitive fallback
        if (bpmField == null && bpmProp == null)
        {
            foreach (var f in t.GetFields(flags))
            {
                if (string.Equals(f.Name, bpmMemberName, StringComparison.OrdinalIgnoreCase))
                {
                    bpmField = f;
                    break;
                }
            }

            if (bpmField == null)
            {
                foreach (var p in t.GetProperties(flags))
                {
                    if (string.Equals(p.Name, bpmMemberName, StringComparison.OrdinalIgnoreCase))
                    {
                        bpmProp = p;
                        break;
                    }
                }
            }
        }

        if (logBindingDebug)
        {
            if (bpmField != null)
            {
                Debug.Log($"[RadialBpmWheel] Bound BPM to FIELD '{bpmField.Name}' (Type: {bpmField.FieldType.Name}) on {t.Name}.", this);
            }
            else if (bpmProp != null)
            {
                Debug.Log($"[RadialBpmWheel] Bound BPM to PROPERTY '{bpmProp.Name}' (Type: {bpmProp.PropertyType.Name}, CanWrite: {bpmProp.CanWrite}) on {t.Name}.", this);
            }
            else
            {
                string fieldNames = string.Join(", ", Array.ConvertAll(t.GetFields(flags), f => $"{f.Name}:{f.FieldType.Name}"));
                string propNames = string.Join(", ", Array.ConvertAll(t.GetProperties(flags), p => $"{p.Name}:{p.PropertyType.Name}(set:{p.CanWrite})"));

                Debug.LogError(
                    $"[RadialBpmWheel] Could not find '{bpmMemberName}' (field or property) on {t.Name}.\n" +
                    $"Fields: {fieldNames}\nProperties: {propNames}\n\n" +
                    $"Fix: set 'bpmMemberName' in the Inspector to the EXACT member name shown above.",
                    this
                );
            }
        }

        // Extra safety: if it found a property but it's not writable, treat as not found.
        if (bpmProp != null && !bpmProp.CanWrite)
        {
            if (logBindingDebug)
                Debug.LogError($"[RadialBpmWheel] Found property '{bpmProp.Name}' but it has no setter. Use a field, or add a public setter.", this);
            bpmProp = null;
        }
    }

    private void SetSequencerBpm(float bpm, bool force)
    {
        if (sequencerComponent == null) return;

        if (bpmField == null && bpmProp == null)
        {
            if (logBindingDebug)
                Debug.LogWarning("[RadialBpmWheel] BPM not set because no binding exists (field/property not found).", this);
            return;
        }

        if (bpmIsInt)
        {
            int v = Mathf.RoundToInt(bpm);

            if (bpmField != null)
            {
                object cur = bpmField.GetValue(sequencerComponent);
                if (!force && cur is int ci && ci == v) return;
                bpmField.SetValue(sequencerComponent, v);
            }
            else
            {
                object cur = bpmProp.GetValue(sequencerComponent);
                if (!force && cur is int ci && ci == v) return;
                bpmProp.SetValue(sequencerComponent, v);
            }
        }
        else
        {
            // Float BPM, but we try to be tolerant (int/float underlying type)
            if (bpmField != null)
            {
                object cur = bpmField.GetValue(sequencerComponent);

                float curF = cur is int ci ? ci : (cur is float cf ? cf : float.NaN);
                if (!force && !float.IsNaN(curF) && Mathf.Approximately(curF, bpm)) return;

                if (bpmField.FieldType == typeof(int))
                    bpmField.SetValue(sequencerComponent, Mathf.RoundToInt(bpm));
                else if (bpmField.FieldType == typeof(float))
                    bpmField.SetValue(sequencerComponent, bpm);
                else
                    Debug.LogWarning($"[RadialBpmWheel] Bound field type is {bpmField.FieldType.Name}. Expected int or float.", this);
            }
            else
            {
                object cur = bpmProp.GetValue(sequencerComponent);

                float curF = cur is int ci ? ci : (cur is float cf ? cf : float.NaN);
                if (!force && !float.IsNaN(curF) && Mathf.Approximately(curF, bpm)) return;

                if (bpmProp.PropertyType == typeof(int))
                    bpmProp.SetValue(sequencerComponent, Mathf.RoundToInt(bpm));
                else if (bpmProp.PropertyType == typeof(float))
                    bpmProp.SetValue(sequencerComponent, bpm);
                else
                    Debug.LogWarning($"[RadialBpmWheel] Bound property type is {bpmProp.PropertyType.Name}. Expected int or float.", this);
            }
        }
    }

    private static Transform[] GetAllChildren(Transform root)
    {
        int count = root.childCount;
        Transform[] arr = new Transform[count];
        for (int i = 0; i < count; i++)
            arr[i] = root.GetChild(i);
        return arr;
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Tube List From ANIMATION (Direct Children)")]
    private void RebuildTubeList()
    {
        if (animationGroup == null)
        {
            Debug.LogError("[RadialBpmWheel] animationGroup not set.");
            return;
        }

        tubes = GetAllChildren(animationGroup);
        tubeStartLocalRots = new Quaternion[tubes.Length];
        for (int i = 0; i < tubes.Length; i++)
            tubeStartLocalRots[i] = tubes[i] != null ? tubes[i].localRotation : Quaternion.identity;

        Debug.Log($"[RadialBpmWheel] Rebuilt tubes list: {tubes.Length} tubes found.");
    }

    [ContextMenu("Re-capture Start Rotations (Wheel + Tubes)")]
    private void RecaptureStarts()
    {
        if (mainRad != null) mainRadStartLocalRot = mainRad.localRotation;

        if (tubes != null)
        {
            tubeStartLocalRots = new Quaternion[tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
                tubeStartLocalRots[i] = tubes[i] != null ? tubes[i].localRotation : Quaternion.identity;
        }

        Debug.Log("[RadialBpmWheel] Captured current wheel + tube rotations as start rotations.");
    }
#endif
}
