using UnityEngine;

public class UpdateSliderFromSetting : MonoBehaviour
{
    
    [SerializeField] private UnityEngine.UI.Slider slider;
    [SerializeField] private string settingName;

    private void Start()
    {
        if (slider != null)
        {
            float value = PlayerPrefs.GetFloat(settingName, 1f); // Default to 1f if not set
            slider.value = value;
        }
    }

}
