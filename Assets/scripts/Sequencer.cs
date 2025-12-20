using UnityEngine;
using FMODUnity;   // FMOD integration namespace

public class Sequencer : MonoBehaviour
{
    [Header("Steps")]
    public SequencerStep[] steps;   // assign all 16 spheres in order

    [Header("Timing")]
    [Tooltip("Beats per minute")]
    public float bpm = 120f;

    [Tooltip("Steps per beat (4 = 16th notes)")]
    public int stepsPerBeat = 4;

    [Header("Sound")]
    [Tooltip("FMOD event to play for active steps")]
    public EventReference stepEvent;   // assign your Acid_Base event here

    private float stepDuration;     // seconds per step
    private double nextStepTime;    // absolute time for next step
    private int currentStep = 0;

    void Start()
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.LogError("Sequencer: No steps assigned!");
            enabled = false;
            return;
        }

        UpdateStepDuration();
        nextStepTime = Time.timeAsDouble;   // start immediately
    }

    void Update()
    {
        // Recalculate step duration so BPM can change at runtime
        UpdateStepDuration();

        double now = Time.timeAsDouble;

        // Catch up if we lagged behind (prevents drift)
        while (now >= nextStepTime)
        {
            TickStep();
            nextStepTime += stepDuration;
        }
    }

    void UpdateStepDuration()
    {
        // Protect against zero or negative BPM
        float safeBpm = Mathf.Max(1f, bpm);
        stepDuration = 60f / safeBpm / Mathf.Max(1, stepsPerBeat);
    }

    void TickStep()
    {
        var step = steps[currentStep];

        // Visual highlight
        step.HighlightStep(stepDuration);

        // Trigger sound only if step is active and event is valid
        if (step.isActive && stepEvent.IsNull == false)
        {
            RuntimeManager.PlayOneShot(stepEvent, transform.position);
        }

        // Advance step index
        currentStep = (currentStep + 1) % steps.Length;
    }
}
