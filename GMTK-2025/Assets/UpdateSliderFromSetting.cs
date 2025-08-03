using UnityEngine;

public class UpdateSliderFromSetting : MonoBehaviour
{
    
    [SerializeField] private UnityEngine.UI.Slider slider;
    [SerializeField] private string settingName;

    private void Awake()
    {
        if (slider != null)
        {
            float value = PlayerPrefs.GetFloat(settingName, 1f);
            // Debug.Log($"Setting {settingName} to {value}");
            slider.value = value;
        }
    }

}
