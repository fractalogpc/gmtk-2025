using UnityEngine;
using UnityEngine.Events;

public class GenericInteractable : MonoBehaviour, IInteractable
{
    public UnityEvent OnInteract;
    public UnityEvent<float> OnHoldInteract;
    public UnityEvent OnReleaseInteract;

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
