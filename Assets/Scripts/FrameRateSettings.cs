using UnityEngine;

using UnityEngine;

public class FrameRateSettings : MonoBehaviour
{
    [SerializeField] private bool useVSync = false;
    [SerializeField] private int targetFps = 120;

    private void Awake()
    {
        if (useVSync)
        {
            QualitySettings.vSyncCount = 1;
            return;
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Max(30, targetFps);
    }
}
