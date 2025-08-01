using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        public Sprite Icon;
        public string ColorName;
        public int Count;
    }

    [SerializeField] private Upgrade[] upgrades;
    [SerializeField] private WoolColorCount[] woolColorCounts;

    [SerializeField] private GameObject[] woolCounters;

    private void UpdateWoolCounters()
    {
        foreach (var woolColorCount in woolColorCounts)
        {
            GameObject reference = woolCounters[woolColorCount.ColorIndex];
            reference.SetActive(woolColorCount.Count > 0);
            reference.GetComponentInChildren<TextMeshProUGUI>().text = woolColorCount.ColorName + ": " + woolColorCount.Count.ToString();
            reference.GetComponentInChildren<Image>().sprite = woolColorCount.Icon;
        }
    }

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

        UpdateWoolCounters();
    }

    public void DepositWool(int colorIndex, int count)
    {
        if (colorIndex < 0 || colorIndex >= woolColorCounts.Length)
        {
            Debug.LogError("Invalid color index for wool deposit.");
            return;
        }

        woolColorCounts[colorIndex].Count += count;
        UpdateWoolCounters();
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