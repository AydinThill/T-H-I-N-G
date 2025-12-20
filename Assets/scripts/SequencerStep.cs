using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class SequencerStep : MonoBehaviour
{
    [Header("Materials")]
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Emission Settings")]
    public Color highlightColor = Color.white;
    public float highlightIntensity = 2f;

    [Header("Interaction")]
    [Tooltip("Tag used by the hand collider to toggle this step")]
    public string handTag = "Hand";

    private Renderer rend;
    private Material runtimeMat;

    [HideInInspector] public bool isActive = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        runtimeMat = rend != null ? rend.material : null;

        if (runtimeMat != null)
        {
            runtimeMat.EnableKeyword("_EMISSION");
            runtimeMat.SetColor("_EmissionColor", Color.black);
        }

        // Important: DO NOT force this collider to be trigger.
        // Let the hand be trigger, this can stay a normal collider.
        // var col = GetComponent<Collider>();
        // col.isTrigger = true;
    }

    void Start()
    {
        if (runtimeMat == null)
        {
            Debug.LogError("SequencerStep: runtimeMat is null on " + gameObject.name);
            enabled = false;
            return;
        }

        UpdateVisual();
    }

    // Mouse click for editor / non-VR testing
    void OnMouseDown()
    {
        ToggleActive();
    }

    // Called when a trigger collider (the hand) enters this sphere's collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(handTag))
        {
            ToggleActive();
            // Debug.Log($"Step {name} toggled by hand ? {(isActive ? "ON" : "OFF")}");
        }
    }

    private void ToggleActive()
    {
        isActive = !isActive;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (runtimeMat == null) return;

        runtimeMat.color = isActive ? activeMaterial.color : inactiveMaterial.color;
        runtimeMat.SetColor("_EmissionColor", Color.black);
    }

    public void HighlightStep(float duration)
    {
        if (runtimeMat == null) return;

        StartCoroutine(FlashEmission(duration));
    }

    private System.Collections.IEnumerator FlashEmission(float duration)
    {
        runtimeMat.SetColor("_EmissionColor", highlightColor * highlightIntensity);
        yield return new WaitForSeconds(duration);
        runtimeMat.SetColor("_EmissionColor", Color.black);
    }
}
