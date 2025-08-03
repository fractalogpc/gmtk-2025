using UnityEngine;

public class StoreSettingValue : MonoBehaviour
{

    [SerializeField] private string settingName;

    public void SetValue(float value)
    {
        PlayerPrefs.SetFloat(settingName, value);
    }
    
}
