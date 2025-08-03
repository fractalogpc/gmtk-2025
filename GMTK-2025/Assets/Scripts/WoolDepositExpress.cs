using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class WoolDepositExpress : MonoBehaviour, IInteractable
{

    [SerializeField] private Animation anim;
    [SerializeField] private ParticleSystem finishEffect;
    [SerializeField] private Outline outlineScript;
    [SerializeField] private StudioEventEmitter depositSoundEmitter;
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
            anim.gameObject.GetComponentInChildren<Renderer>().material = SheepDataHolder.Instance.sheeps[InventoryController.Instance.SelectedWoolColorIndex].color;

            InventoryController.Instance.TryRemoveItem(slot);


            StartCoroutine(DepositWool());
        }
    }

    private IEnumerator DepositWool()
    {
        isDepositing = true;
        depositSoundEmitter.Play();
        anim.Play();
        yield return new WaitForSeconds(anim.clip.length);
        finishEffect.Play();
        isDepositing = false;
    }

    public void OnHoverEnter()
    {
        Color outlineColor = outlineScript.OutlineColor;
        outlineColor.a = 1f;
        outlineScript.OutlineColor = outlineColor;
    }

    public void OnHoverExit()
    {
        Color outlineColor = outlineScript.OutlineColor;
        outlineColor.a = 0f;
        outlineScript.OutlineColor = outlineColor;
    }

    public void HoldInteract(float holdTime) { }

    public void ReleaseInteract() { }

}
