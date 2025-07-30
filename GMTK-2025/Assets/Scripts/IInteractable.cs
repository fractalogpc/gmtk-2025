using UnityEngine;

public interface IInteractable
{
    void Interact();

    void HoldInteract(float holdTime);

    void ReleaseInteract();
}
