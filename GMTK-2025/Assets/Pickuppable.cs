using UnityEngine;

public class Pickuppable : MonoBehaviour, IInteractable
{

    public InventoryController.ItemType itemType;

    public void HoldInteract(float holdTime)
    {
        
    }

    public void Interact()
    {
        if (InventoryController.Instance.GetNextAvailableSlot(out int index))
        {
            InventoryController.Instance.TryAddItem(itemType, index);
            Destroy(gameObject);
        }
    }

    public void OnHoverEnter()
    {
        
    }

    public void OnHoverExit()
    {
        
    }

    public void ReleaseInteract()
    {
        
    }
}
