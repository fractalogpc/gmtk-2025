using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WoolDeposit : MonoBehaviour, IInteractable
{

    [SerializeField] private Animation anim;
    [SerializeField] private float timePerWool = 1f;
    [SerializeField] private ParticleSystem washEffect;

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

        // Otherwise play the close animation and empty the machine

        if (false)
        {
            // Deposit the wool
        }
        else
        {
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

        // After washing, clear the deposits
        woolDeposits.Clear();

        anim.Play("woolotrondooropen");
        yield return new WaitForSeconds(anim.clip.length);
    }
    
}
