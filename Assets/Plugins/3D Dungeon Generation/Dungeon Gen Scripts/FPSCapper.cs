using UnityEngine;

public class FPSCapper : MonoBehaviour
{
    void Awake()
    {
#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 100;
#endif
    }
}
