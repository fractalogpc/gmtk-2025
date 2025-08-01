using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WoolDepositExpress : MonoBehaviour, IInteractable
{

    [SerializeField] private Animation anim;
    [SerializeField] private ParticleSystem finishEffect;
    [SerializeField] private MonoBehaviour outlineScript;
    private bool isDepositing = false;

    public void Interact()
    {
        // If player is holding wool, deposit it
        if (InventoryController.Instance.IsHoldingObject(InventoryController.ItemType.Wool))
        {
            if (isDepositing)
            {
                Debug.Log("Cannot deposit wool while using!");
                return;
            }
            int slot = InventoryController.Instance.SelectedSlot;
            UpgradeManager.Instance.DepositWool(InventoryController.Instance.SelectedWoolColorIndex, InventoryController.Instance.SelectedWoolSize);
            InventoryController.Instance.TryRemoveItem(slot);

            StartCoroutine(DepositWool());
        }
    }

    private IEnumerator DepositWool()
    {
        isDepositing = true;
        anim.Play();
        yield return new WaitForSeconds(anim.clip.length);
        finishEffect.Play();
        isDepositing = false;
    }

    public void OnHoverEnter()
    {
        outlineScript.enabled = true;
    }

    public void OnHoverExit()
    {
        outlineScript.enabled = false;
    }

    public void HoldInteract(float holdTime) { }

    public void ReleaseInteract() { }

}
