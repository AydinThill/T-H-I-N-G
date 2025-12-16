using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerToggleWithHandCubeConsoleClean : MonoBehaviour
{
    [Header("XR Trigger Input (Activate)")]
    public InputActionProperty triggerAction;

    [Header("Raw Trigger Value (0–1)")]
    [Range(0f, 1f)]
    public float triggerValue = 0f;

    [Header("Toggle Value (0 or 1)")]
    [Range(0, 1)]
    public int toggleValue = 0;

    [Header("Threshold to register toggle press")]
    public float toggleThreshold = 0.8f;

    private bool handInside = false;
    private bool triggerWasPressedLastFrame = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            handInside = true;
            Debug.Log("Hand cube entered sphere.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            handInside = false;
            triggerValue = 0f;
            triggerWasPressedLastFrame = false;
            Debug.Log("Hand cube exited sphere. Values reset.");
        }
    }

    private void Update()
    {
        if (!handInside || triggerAction.action == null) return;

        // --- Read raw trigger value (0–1) ---
        triggerValue = Mathf.Clamp01(triggerAction.action.ReadValue<float>());

        // --- Toggle logic ---
        bool triggerPressed = triggerValue >= toggleThreshold;

        if (triggerPressed && !triggerWasPressedLastFrame)
        {
            toggleValue = toggleValue == 0 ? 1 : 0;
            // Only print when the toggle value actually changes
            Debug.Log($"Toggle Value Changed → {toggleValue}");
        }

        triggerWasPressedLastFrame = triggerPressed;
    }
}

