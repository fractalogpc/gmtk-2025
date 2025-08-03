using UnityEngine;

public class ToolPointManager : MonoBehaviour, IInteractable
{

    [SerializeField] private Renderer[] placedRenderers;
    [SerializeField] private Renderer[] removedRenderers;
    [SerializeField] private Outline[] outlineScript;
    [SerializeField] private InventoryController.ItemType toolType;
    [SerializeField] private Material[] toolLevelMaterials;

    public string InteractionName => isPlaced ? "Pick Up" : "Place";

    private bool isPlaced = true;
    private int toolLevel = 0;

    public void OnHoverEnter()
    {
        foreach (var outline in outlineScript)
        {
            Color outlineColor = outline.OutlineColor;
            outlineColor.a = 1f;
            outline.OutlineColor = outlineColor;
        }
    }

    public void OnHoverExit()
    {
        foreach (var outline in outlineScript)
        {
            Color outlineColor = outline.OutlineColor;
            outlineColor.a = 0f;
            outline.OutlineColor = outlineColor;
        }
    }

    public void SetToolLevel(int level)
    {
        if (level < 0 || level > toolLevelMaterials.Length)
        {
            Debug.LogError("Invalid tool level: " + level);
            return;
        }

        toolLevel = level;
        UpdateToolLevels();
    }

    public void UpdateToolLevels()
    {
        if (toolLevel <= 0 || toolLevel > toolLevelMaterials.Length)
        {
            Debug.LogError("Invalid tool level: " + toolLevel);
            return;
        }

        for (int i = 0; i < placedRenderers.Length; i++)
        {
            placedRenderers[i].material = toolLevelMaterials[toolLevel - 1];
        }
    }

    public void Interact()
    {
        if (isPlaced)
        {
            // Try to add to player's inventory
            if (!InventoryController.Instance.GetNextAvailableSlot(out int slot))
            {
                Debug.Log("No available inventory slots!");
                return;
            }

            // Add tool to inventory
            InventoryController.Instance.TryAddItem(toolType, slot, level: toolLevel);

            // Swap renderers
            foreach (var renderer in placedRenderers)
            {
                renderer.enabled = false;
            }
            foreach (var renderer in removedRenderers)
            {
                renderer.enabled = true;
            }
            isPlaced = false;
        }
        else
        {
            // Check if the player is holding the tool
            if (!InventoryController.Instance.IsHoldingObject(toolType))
            {
                return;
            }

            // Remove from player's inventory
            InventoryController.Instance.TryRemoveItem(InventoryController.Instance.SelectedSlot);

            // Swap renderers
            foreach (var renderer in removedRenderers)
            {
                renderer.enabled = false;
            }
            foreach (var renderer in placedRenderers)
            {
                renderer.enabled = true;
            }
            isPlaced = true;
        }
    }

    public void HoldInteract(float holdTime) { }
    public void ReleaseInteract() { }
    
}
