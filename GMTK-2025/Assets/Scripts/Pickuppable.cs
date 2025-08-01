using Unity.VisualScripting;
using UnityEngine;

public class Pickuppable : MonoBehaviour, IInteractable
{

    public InventoryController.ItemType itemType;
    public int woolColorIdx = 0;
    public int woolSize = 1;

    public void HoldInteract(float holdTime)
    {

    }

    public void Interact()
    {
        if (InventoryController.Instance.GetNextAvailableSlot(out int index))
        {

            // Wool is different
            if (itemType == InventoryController.ItemType.Wool)
            {
                InventoryController.Instance.TryAddItem(itemType, index, woolColorIdx, woolSize);
                GameObject parent = transform.parent.gameObject;
                Destroy(parent);
            }
            else
            {
                InventoryController.Instance.TryAddItem(itemType, index);
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
