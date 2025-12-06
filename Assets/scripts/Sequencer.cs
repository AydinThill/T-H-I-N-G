using UnityEngine;

public class Sequencer : MonoBehaviour
{
    public SequencerStep[] steps; // assign all 16 spheres
    public float bpm = 120f;

    private float stepDuration;
    private int currentStep = 0;

    void Start()
    {
        UpdateStepDuration();
        StartCoroutine(RunSequencer());
    }

    void UpdateStepDuration()
    {
        // 16th note by default → 4 steps per beat
        stepDuration = 60f / bpm / 4f;
    }

    void Update()
    {
        // dynamic bpm
        UpdateStepDuration();
    }

    private System.Collections.IEnumerator RunSequencer()
    {
        while (true)
        {
            // Highlight current step
            steps[currentStep].HighlightStep(stepDuration);

            // If step is active → trigger sound later with FMOD
            if (steps[currentStep].isActive)
            {
                // FMOD hook (to be added later)
                // PlayStepSound(currentStep);
            }

            currentStep = (currentStep + 1) % steps.Length;

            yield return new WaitForSeconds(stepDuration);
        }
    }
}
