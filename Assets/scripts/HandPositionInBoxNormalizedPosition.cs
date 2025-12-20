using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class HandPositionInBoxNormalizedPosition : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference growlEvent; // set to event:/SkrillexAlienGrowl

    [Header("FMOD Parameter Names (must match FMOD exactly)")]
    public string paramFormVowel = "form_vowel";
    public string paramFormShift = "form_shift";
    public string paramFreqHz = "freq_hz";

    [Header("Output: Raw Local Position (X,Y,Z)")]
    public Vector3 handPosLocal;

    [Header("Output: Normalized Position (0–1 on each axis)")]
    public Vector3 handPosNormalized;

    private Transform handInside;
    private BoxCollider box;

    private EventInstance instance;
    private bool instanceValid;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        if (box == null)
            Debug.LogError("This script needs to be on an object with a BoxCollider.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;

        handInside = other.transform;

        if (!instanceValid || !instance.isValid())
        {
            if (growlEvent.IsNull)
            {
                Debug.LogError("FMOD EventReference is not set (growlEvent).");
                return;
            }

            instance = RuntimeManager.CreateInstance(growlEvent);

            // Sound positioned at the box (switch to handInside.gameObject if desired)
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject);

            instance.start();
            instanceValid = true;
        }

        Debug.Log("Hand entered the box.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (handInside == null) return;
        if (other.transform != handInside) return;

        Debug.Log("Hand exited the box.");

        handInside = null;
        handPosLocal = Vector3.zero;
        handPosNormalized = Vector3.zero;

        if (instanceValid && instance.isValid())
        {
            instance.setParameterByName(paramFormVowel, 0f);
            instance.setParameterByName(paramFormShift, 0f);
            instance.setParameterByName(paramFreqHz, 0f);

            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            instance.clearHandle();
        }
        instanceValid = false;
    }

    private void OnDisable()
    {
        if (instanceValid && instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
            instance.clearHandle();
        }
        instanceValid = false;
    }

    private void Update()
    {
        if (handInside == null) return;
        if (!instanceValid || !instance.isValid()) return;

        Vector3 localPos = transform.InverseTransformPoint(handInside.position);
        handPosLocal = localPos;

        Vector3 size = box.size;

        float nx = Mathf.InverseLerp(-size.x / 2f, size.x / 2f, localPos.x);
        float ny = Mathf.InverseLerp(-size.y / 2f, size.y / 2f, localPos.y);
        float nz = Mathf.InverseLerp(-size.z / 2f, size.z / 2f, localPos.z);

        handPosNormalized = new Vector3(nx, ny, nz);

        instance.setParameterByName(paramFormVowel, nx);
        instance.setParameterByName(paramFormShift, ny);
        instance.setParameterByName(paramFreqHz, nz);
    }
}
