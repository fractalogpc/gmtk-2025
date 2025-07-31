using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [System.Serializable]
    private class Upgrade
    {

        public string Name;
        public GameObject[] EnabledObjects;
        public GameObject[] DisabledObjects;
        public bool IsOwned;

    }

    [System.Serializable]
    private class WoolColorCount
    {
        public int ColorIndex;
        public int Count;
    }

    [SerializeField] private Upgrade[] upgrades;
    [SerializeField] private WoolColorCount[] woolColorCounts;

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

    private void Start()
    {
        foreach (var upgrade in upgrades)
        {
            UpdateUpgradeState(upgrade);
        }
    }

    private void UpdateUpgradeState(Upgrade upgrade)
    {
        if (upgrade.IsOwned)
        {
            foreach (var obj in upgrade.EnabledObjects)
            {
                obj.SetActive(true);
            }
            foreach (var obj in upgrade.DisabledObjects)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            foreach (var obj in upgrade.EnabledObjects)
            {
                obj.SetActive(false);
            }
            foreach (var obj in upgrade.DisabledObjects)
            {
                obj.SetActive(true);
            }
        }
    }
    
}