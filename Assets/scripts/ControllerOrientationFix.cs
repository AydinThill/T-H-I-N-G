using UnityEngine;

public class ControllerOrientationFix : MonoBehaviour
{
    [Header("Controller Visuals")]
    public Transform rightControllerVisual;
    public Transform rightAffordanceCallouts;
    public Transform leftControllerVisual;
    public Transform leftAffordanceCallouts;

    [Header("Quest 2 Fix")]
    [Tooltip("Enable this on Quest 2 to flip the controllers 180° around Z.")]
    public bool useQuest2Fix = true;

    private Quaternion rightVisOriginal;
    private Quaternion rightVisFlipped;
    private Quaternion rightAffOriginal;
    private Quaternion rightAffFlipped;

    private Quaternion leftVisOriginal;
    private Quaternion leftVisFlipped;
    private Quaternion leftAffOriginal;
    private Quaternion leftAffFlipped;

    private bool lastAppliedState;

    void Awake()
    {
        // Cache original rotations
        if (rightControllerVisual != null)
        {
            rightVisOriginal = rightControllerVisual.localRotation;
            rightVisFlipped = rightVisOriginal * Quaternion.Euler(0f, 0f, 180f);
        }

        if (rightAffordanceCallouts != null)
        {
            rightAffOriginal = rightAffordanceCallouts.localRotation;
            rightAffFlipped = rightAffOriginal * Quaternion.Euler(0f, 0f, 180f);
        }

        if (leftControllerVisual != null)
        {
            leftVisOriginal = leftControllerVisual.localRotation;
            leftVisFlipped = leftVisOriginal * Quaternion.Euler(0f, 0f, 180f);
        }

        if (leftAffordanceCallouts != null)
        {
            leftAffOriginal = leftAffordanceCallouts.localRotation;
            leftAffFlipped = leftAffOriginal * Quaternion.Euler(0f, 0f, 180f);
        }

        ApplyFix(useQuest2Fix);
        lastAppliedState = useQuest2Fix;
    }

    void Update()
    {
        // If you toggle the bool in the Inspector or from another script at runtime,
        // this will re-apply the correct orientation.
        if (useQuest2Fix != lastAppliedState)
        {
            ApplyFix(useQuest2Fix);
            lastAppliedState = useQuest2Fix;
        }
    }

    public void ApplyFix(bool enable)
    {
        if (rightControllerVisual != null)
            rightControllerVisual.localRotation = enable ? rightVisFlipped : rightVisOriginal;

        if (rightAffordanceCallouts != null)
            rightAffordanceCallouts.localRotation = enable ? rightAffFlipped : rightAffOriginal;

        if (leftControllerVisual != null)
            leftControllerVisual.localRotation = enable ? leftVisFlipped : leftVisOriginal;

        if (leftAffordanceCallouts != null)
            leftAffordanceCallouts.localRotation = enable ? leftAffFlipped : leftAffOriginal;
    }
}
