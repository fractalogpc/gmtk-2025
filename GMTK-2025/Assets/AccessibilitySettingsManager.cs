using UnityEngine;

public class AccessibilitySettingsManager : MonoBehaviour
{

    [SerializeField] private Material[] strobingMaterials;

    public void UpdateStrobingSettings()
    {
        bool isStrobingEnabled = PlayerPrefs.GetInt("Strobing", 1) == 1;

        foreach (Material mat in strobingMaterials)
        {
            mat.SetFloat("_Brightness", isStrobingEnabled ? 0.01f : 0);
        }
    }
}
