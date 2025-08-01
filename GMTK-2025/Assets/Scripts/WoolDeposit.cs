using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WoolDeposit : MonoBehaviour, IInteractable
{

    [SerializeField] private Animation anim;
    [SerializeField] private float timeToWash = 1f;
    [SerializeField] private ParticleSystem washEffect;
    [SerializeField] private GameObject woolDepositPrefab;
    [SerializeField] private Transform[] depositPoints;
    [SerializeField] private Transform woolDepositParent;
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private AnimationCurve spinCurve;
    [SerializeField] private ParticleSystem finishEffect;

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
                Size = InventoryController.Instance.SelectedWoolSize,
                Type = InventoryController.Instance.SelectedWoolColorIndex
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
        yield return StartCoroutine(SpinCycle(timeToWash));
        washEffect.Stop();
        finishEffect.Play();
        isWashing = false;

        // Deposit wool after washing
        for (int i = 0; i < woolDeposits.Count; i++)
        {
            var deposit = woolDeposits[i];
            UpgradeManager.Instance.DepositWool(deposit.Type, deposit.Size);
            // Delete the deposit object
            Destroy(depositPoints[i].GetChild(0).gameObject);
        }
        // After washing, clear the deposits
        woolDeposits.Clear();

        anim.Play("woolotrondooropen");
        yield return new WaitForSeconds(anim.clip.length);
    }

    private IEnumerator SpinCycle(float duration)
    {
        float elapsed = 0f;
        float angle = 0f;
        Quaternion initialRotation = woolDepositParent.localRotation;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            angle += spinCurve.Evaluate(t) * spinSpeed * Time.deltaTime;
            woolDepositParent.localRotation = initialRotation * Quaternion.Euler(angle, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
}
