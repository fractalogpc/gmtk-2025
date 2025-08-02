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

    private int currentSheepColorIndex = 0;
    private int currentSheepSize = 1;

    void Start()
    {
        cloningSheep.SetActive(false);
    }

    private void UpdateSheepInfo()
    {
        sheepColorText.text = colorNames[currentSheepColorIndex];
        sheepSizeText.text = "Size " + currentSheepSize.ToString();
        estimatedTimeText.text = "Estimated time/sheep: " + GetEstimatedCloningTime(currentSheepColorIndex, currentSheepSize).ToString("F2") + " sec";
    }

    private float GetEstimatedCloningTime(int colorIndex, float size)
    {
        if (colorIndex < 0 || colorIndex >= cloningTimesByColorIndex.Length)
        {
            return 0f; // Invalid color index
        }
        return cloningTimesByColorIndex[colorIndex] * ((size - 1.0f) * cloningTimeSizeMultiplier);
    }

    public void TryAddSheepToChamber()
    {
        InventoryController inventory = InventoryController.Instance;

        if (inventory.IsHoldingObject(InventoryController.ItemType.Sheep))
        {
            AdvancedSheepController sheep = inventory.GetHeldSheep();
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
            Destroy(sheep.gameObject);
        }
    }
    
}
