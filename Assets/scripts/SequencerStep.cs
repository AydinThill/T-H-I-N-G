using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SequencerStep : MonoBehaviour
{
    [Header("Materials")]
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Emission Settings")]
    public Color highlightColor = Color.white;
    public float highlightIntensity = 2f;

    private Renderer rend;
    public bool isActive = false;
    private Material runtimeMat;

    void Awake()
    {
        rend = GetComponent<Renderer>();            
        runtimeMat = rend != null ? rend.material : null;
    }

    void Start()
    {
        if (runtimeMat == null)
        {
            Debug.LogError("SequencerStep: runtimeMat is null on " + gameObject.name);
            return;
        }
        UpdateVisual();
    }

    void OnMouseDown()
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
