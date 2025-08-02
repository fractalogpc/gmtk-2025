using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [System.Serializable]
    private struct WoolColorCost
    {
        public int ColorIndex;
        public int Count;
    }

    [System.Serializable]
    private class Upgrade
    {

        public string Name;
        public bool StartOwned;
        public GameObject OptionalSignObjectToDestroyIfStartOwned;
        public GameObject[] EnabledObjects;
        public GameObject[] DisabledObjects;
        public bool IsOwned;
        public WoolColorCost[] WoolCosts;
        public UnityEvent OnUnlockEvent;

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool OwnsUpgrade(string upgradeName)
    {
        Upgrade upgrade = System.Array.Find(upgrades, u => u.Name == upgradeName);
        return upgrade != null && upgrade.IsOwned;
    }

    private void Start()
    {
        foreach (var upgrade in upgrades)
        {
            UpdateUpgradeState(upgrade);
        }

        UpdateWoolCounters();

        foreach (var upgrade in upgrades)
        {
            if (upgrade.StartOwned)
            {
                upgrade.IsOwned = true;
                UpdateUpgradeState(upgrade);
                if (upgrade.OptionalSignObjectToDestroyIfStartOwned != null)
                {
                    Destroy(upgrade.OptionalSignObjectToDestroyIfStartOwned);
                }
                upgrade.OnUnlockEvent?.Invoke();
            }
        }
    }
    
    public void RemoveWool(int colorIndex, int count)
    {
        if (colorIndex < 0 || colorIndex >= woolColorCounts.Length)
        {
            Debug.LogError("Invalid color index for wool removal.");
            return;
        }
        if (woolColorCounts[colorIndex].Count >= count)
        {
            woolColorCounts[colorIndex].Count -= count;
            UpdateWoolCounters();
        }
        else
        {
            Debug.LogWarning("Not enough wool to remove: " + count + " of color index " + colorIndex);
        }
    }

    public bool TryBuyUpgrade(string upgradeName)
    {
        Upgrade upgrade = System.Array.Find(upgrades, u => u.Name == upgradeName);
        if (upgrade != null && !upgrade.IsOwned)
        {
            bool canAfford = true;
            foreach (var cost in upgrade.WoolCosts)
            {
                if (cost.ColorIndex < 0 || cost.ColorIndex >= woolColorCounts.Length || woolColorCounts[cost.ColorIndex].Count < cost.Count)
                {
                    canAfford = false;
                    break;
                }
            }
            if (canAfford)
            {
                // Deduct the wool costs
                foreach (var cost in upgrade.WoolCosts)
                {
                    woolColorCounts[cost.ColorIndex].Count -= cost.Count;
                }

                upgrade.IsOwned = true;
                UpdateUpgradeState(upgrade);
                // Debug.Log("Upgrade purchased: " + upgradeName);
                UpdateWoolCounters();
                upgrade.OnUnlockEvent?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough wool to purchase upgrade: " + upgradeName);
                return false;
            }
        }
        else
        {
            Debug.LogWarning("Upgrade not found or already owned: " + upgradeName);
            return false;
        }
    }

    public void DepositWool(int colorIndex, int count)
    {
        if (colorIndex < 0 || colorIndex >= woolColorCounts.Length)
        {
            Debug.LogError("Invalid color index for wool deposit.");
            return;
        }
        if (OwnsUpgrade("Wool-O-Tron #1"))
        {
            count *= 2; // Double the count if the upgrade is owned
        }
        woolColorCounts[colorIndex].Count += count;
        UpdateWoolCounters();
    }

    public int GetWoolCount(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= woolColorCounts.Length)
        {
            Debug.LogError("Invalid color index for wool count retrieval.");
            return 0;
        }
        return woolColorCounts[colorIndex].Count;
    }

    private void UpdateUpgradeState(Upgrade upgrade)
    {
        if (upgrade.IsOwned)
        {
            foreach (var obj in upgrade.EnabledObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            foreach (var obj in upgrade.DisabledObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
        else
        {
            foreach (var obj in upgrade.EnabledObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
            // foreach (var obj in upgrade.DisabledObjects)
            // {
            //     if (obj != null)
            //     {
            //         obj.SetActive(true);
            //     }
            // }
        }
    }
    
}