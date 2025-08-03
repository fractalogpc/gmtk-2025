using UnityEngine;

public class UpdateCheckBoxFromSetting : MonoBehaviour
{

    [SerializeField] private UnityEngine.UI.Toggle toggle;
    [SerializeField] private string settingName;

    private void Start()
    {
        if (toggle != null)
        {
            int value = PlayerPrefs.GetInt(settingName, 1); // Default to 0 (unchecked) if not set
            toggle.isOn = value == 1;
        }
    }
    
}
