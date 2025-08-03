using UnityEngine;
using TMPro;

public class CloningManager : MonoBehaviour
{

    [SerializeField] private CloningChamber cloningChamber1;
    [SerializeField] private CloningChamber cloningChamber2;

    [SerializeField] private TextMeshProUGUI sheepColorText;
    [SerializeField] private TextMeshProUGUI sheepSizeText;
    [SerializeField] private TextMeshProUGUI estimatedTimeText;

    [SerializeField] private float[] cloningTimesByColorIndex;

    [SerializeField] private float cloningTimeSizeMultiplier = 1.0f;
    [SerializeField] private string[] colorNames;

    [SerializeField] private GameObject cloningSheep;
    [SerializeField] private Renderer[] cloningSheepRenderers;

    public AdvancedSheepController sheepBeingCloned;

    void Start()
    {
        cloningSheep.SetActive(false);
    }

    private void UpdateSheepInfo()
    {
        if (sheepBeingCloned == null) return;

        sheepColorText.text = GetColorName(sheepBeingCloned.woolColorIndex);
        sheepSizeText.text = GetSizeDescription(sheepBeingCloned.woolSize);
        estimatedTimeText.text = GetEstimatedCloningTimeString(sheepBeingCloned.woolColorIndex, sheepBeingCloned.woolSize);
    }

    public void TryAddSheepToChamber()
    {
        InventoryController inventory = InventoryController.Instance;

        if (inventory.IsHoldingObject(InventoryController.ItemType.Sheep))
        {
            AdvancedSheepController sheep = inventory.GetHeldSheep();
            sheepBeingCloned = sheep;
            if (!inventory.TryRemoveItem(inventory.SelectedSlot))
            {
                Debug.LogError("Failed to remove sheep from inventory.");
            }

            cloningChamber1.ReplaceSheep(sheep);
            cloningChamber2.ReplaceSheep(sheep);

            // Set the current sheep color and size
            cloningSheep.SetActive(true);
            Material sheepMaterial = sheep.woolMaterial;
            foreach (Renderer renderer in cloningSheepRenderers)
            {
                renderer.material = sheepMaterial;
            }

            // Destroy held sheep
            SheepSpawner.Instance.RemoveSheep(sheep);
            sheep.gameObject.SetActive(false);

            UpdateSheepInfo();

        }
    }

    // Called when the new cloning chamber is activated
    public void StartNewCloning()
    {
        cloningChamber1.ReplaceSheep(sheepBeingCloned);
        cloningChamber2.ReplaceSheep(sheepBeingCloned);
    }

    private string GetColorName(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0: return "White";
            case 1: return "Gray";
            case 2: return "Brown";
            case 3: return "Pink";
            case 4: return "Gold";
            case 5: return "Rainbow";
            default: return "Unknown";
        }
    }

    private string GetSizeDescription(int size)
    {
        switch (size)
        {
            case 1: return "Small";
            case 2: return "Medium";
            case 3: return "Large";
            default: return "Unknown";
        }
    }

    private string GetEstimatedCloningTimeString(int colorIndex, int size)
    {
        int time = GetEstimatedCloningTime(colorIndex, size);
        return time + " sec";
    }

    private int GetEstimatedCloningTime(int colorIndex, int size)
    {
        switch (colorIndex)
        {
            case 0:
            case 1:
            case 2:
                {
                    switch (size)
                    {
                        case 1:
                            return 25;
                        case 2:
                            return 45;
                        case 3:
                            return 60;
                    }
                }
                break;
            case 3:
            case 4:
            case 5:
                {
                    switch (size)
                    {
                        case 1:
                            return 50;
                        case 2:
                            return 90;
                        case 3:
                            return 120;
                            break;
                    }
                }
                break;
        }

        return 0; // Default case if no match found
    }
}
