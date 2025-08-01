using UnityEngine;

public interface IInteractable
{
    void OnHoverEnter();

    void OnHoverExit();

    void Interact();

    void HoldInteract(float holdTime);

    void ReleaseInteract();
}
