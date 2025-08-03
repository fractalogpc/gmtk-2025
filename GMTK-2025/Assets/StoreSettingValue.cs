using UnityEngine;

public class StoreSettingValue : MonoBehaviour
{

    [SerializeField] private string settingName;

    public void SetValue(float value)
    {
        PlayerPrefs.SetFloat(settingName, value);
    }

    public void SetValue(bool value)
    {
        PlayerPrefs.SetInt(settingName, value ? 1 : 0);
    }

}
