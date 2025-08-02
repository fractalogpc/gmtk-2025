using UnityEngine;

public class SheepDataHolder : MonoBehaviour
{
    public static SheepDataHolder Instance { get; private set; }

    public SheepObject[] sheeps;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
