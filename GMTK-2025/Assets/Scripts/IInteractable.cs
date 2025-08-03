using UnityEngine;

public interface IInteractable
{

    public string InteractionName { get; }

    void OnHoverEnter();

    void OnHoverExit();

    void Interact();

    void HoldInteract(float holdTime);

    void ReleaseInteract();
}
