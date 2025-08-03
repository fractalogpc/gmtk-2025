using UnityEngine;
using UnityEngine.Events;

public class GenericInteractable : MonoBehaviour, IInteractable
{
    public UnityEvent OnHoverEnterEvent;
    public UnityEvent OnHoverExitEvent;
    public UnityEvent OnInteract;
    public UnityEvent<float> OnHoldInteract;
    public UnityEvent OnReleaseInteract;

    [SerializeField] private string interactionName = "Interact";
    public string InteractionName => interactionName;

    public void OnHoverEnter()
    {
        OnHoverEnterEvent?.Invoke();
    }

    public void OnHoverExit()
    {
        OnHoverExitEvent?.Invoke();
    }

    public void Interact()
    {
        OnInteract?.Invoke();
    }

    public void HoldInteract(float holdTime)
    {
        OnHoldInteract?.Invoke(holdTime);
    }

    public void ReleaseInteract()
    {
        OnReleaseInteract?.Invoke();
    }
}
