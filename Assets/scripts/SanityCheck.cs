using UnityEngine;
using FMODUnity;

public class FmodSanityCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"FMOD HaveMasterBanksLoaded: {RuntimeManager.HaveMasterBanksLoaded}");
        Debug.Log($"FMOD HaveAllBanksLoaded: {RuntimeManager.HaveAllBanksLoaded}");

        try
        {
            var guid = RuntimeManager.PathToGUID("event:/Steal_Drum");
            Debug.Log($"FMOD lookup by PATH works. GUID is: {guid}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FMOD lookup by PATH failed: {e.Message}");
        }
    }
}
