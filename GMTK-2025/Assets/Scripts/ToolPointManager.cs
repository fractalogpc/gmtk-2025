using UnityEngine;

public class ToolPointManager : MonoBehaviour, IInteractable
{

    [SerializeField] private Renderer[] placedRenderers;
    [SerializeField] private Renderer[] removedRenderers;
    [SerializeField] private MonoBehaviour outlineScript;
    [SerializeField] private InventoryController.ItemType toolType;

    private bool isPlaced = true;

    void Start()
    {

    }

    void Update()
    {

    }

    public void OnHoverEnter()
    { 
        outlineScript.enabled = true;
    }

    public void OnHoverExit()
    {
        outlineScript.enabled = false;
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
            InventoryController.Instance.TryAddItem(toolType, slot);

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
