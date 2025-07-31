using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WoolDeposit : MonoBehaviour, IInteractable
{

    [SerializeField] private Animation anim;
    [SerializeField] private float timePerWool = 1f;
    [SerializeField] private ParticleSystem washEffect;
    [SerializeField] private GameObject woolDepositPrefab;
    [SerializeField] private Transform[] depositPoints;

    struct DepositedWool
    {
        public int Size;
        public int Type;
    }

    private List<DepositedWool> woolDeposits = new List<DepositedWool>();
    private bool isWashing = false;

    public void Interact()
    {
        // If player is holding wool, deposit it
        if (InventoryController.Instance.IsHoldingObject(InventoryController.ItemType.Wool))
        {
            if (isWashing)
            {
                Debug.Log("Cannot deposit wool while washing!");
                return;
            }
            if (woolDeposits.Count >= depositPoints.Length)
            {
                Debug.Log("No more deposit points available!");
                return;
            }
            int slot = InventoryController.Instance.SelectedSlot;

            DepositedWool wool = new DepositedWool
            {
                Size = 1,
                Type = 0
            };
            woolDeposits.Add(wool);
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject woolDeposit = Instantiate(woolDepositPrefab, depositPoints[woolDeposits.Count - 1].position, randomRotation);
            woolDeposit.transform.SetParent(depositPoints[woolDeposits.Count - 1]);
            InventoryController.Instance.TryRemoveItem(slot);
        }
        else
        {
            // If player is not holding wool, trigger wash cycle
            TriggerWashCycle();
        }
    }

    public void OnHoverEnter() { }

    public void OnHoverExit() { }

    public void HoldInteract(float holdTime) { }

    public void ReleaseInteract() { }

    private void TriggerWashCycle()
    {
        if (isWashing)
        {
            Debug.Log("Already washing!");
            return;
        }

        if (woolDeposits.Count == 0)
        {
            Debug.Log("No wool to wash!");
            return;
        }

        StartCoroutine(WashCycle());
    }

    private IEnumerator WashCycle()
    {
        isWashing = true;
        anim.Play("woolotrondoorclose");
        yield return new WaitForSeconds(anim.clip.length);
        washEffect.Play();
        yield return new WaitForSeconds(timePerWool * woolDeposits.Count);
        washEffect.Stop();
        isWashing = false;

        // Deposit wool after washing TODO!
        foreach (var deposit in woolDeposits)
        {
            // Here you can handle the deposited wool, e.g., increase player's wool count
            // Delete the deposit object
            Destroy(depositPoints[woolDeposits.IndexOf(deposit)].GetChild(0).gameObject);
        }
        // After washing, clear the deposits
        woolDeposits.Clear();

        anim.Play("woolotrondooropen");
        yield return new WaitForSeconds(anim.clip.length);
    }
    
}
