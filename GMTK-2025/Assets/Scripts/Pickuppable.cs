using Unity.VisualScripting;
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

            // Wool is different
            if (itemType == InventoryController.ItemType.Wool)
            {
                GameObject parent = transform.parent.gameObject;
                Destroy(parent);
            }

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
