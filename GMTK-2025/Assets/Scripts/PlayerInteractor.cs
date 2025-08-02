using UnityEngine;

public class PlayerInteractor : InputHandlerBase
{
  [Header("Raycast Settings")]
  public Camera raycastCamera;
  public float raycastDistance = 5f;
  public LayerMask interactableLayer;


  private bool isInteracting = false;
  private GameObject currentInteractableObject;
  private IInteractable currentInteractable;
  private IInteractable previousInteractable;
  private float holdTime = 0f;

  private bool isHovering = false;

  protected override void InitializeActionMap()
  {
    RegisterAction(
      _inputActions.Player.Interact,
      _ => { if (isHovering) { BeginInteract(); } },
      () => { ReleaseInteract(); }
    );

  }

  private void Update()
  {
    UpdateHoverState();

    if (isInteracting)
    {
      HoldInteract();
    }
  }

  private void UpdateHoverState()
  {
    IInteractable hitInteractable = RaycastForInteractable();

    if (hitInteractable != previousInteractable)
    {
      // Exit hover on old
      if (previousInteractable != null)
      {
        previousInteractable.OnHoverExit();
      }

      // Enter hover on new
      if (hitInteractable != null)
      {
        hitInteractable.OnHoverEnter();
      }

      currentInteractable = hitInteractable;
      previousInteractable = currentInteractable;
    }
  }

  private IInteractable RaycastForInteractable()
  {
    Ray ray = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
    if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer, QueryTriggerInteraction.Collide))
    {
      currentInteractableObject = hit.collider.gameObject;
      isHovering = true;
      return hit.collider.GetComponent<IInteractable>();
    }

    currentInteractableObject = null;
    isHovering = false;
    return null;
  }

  private void BeginInteract()
  {
    if (currentInteractable != null)
    {
      holdTime = 0f;
      currentInteractable.Interact();

      isInteracting = true;
    }
  }

  private void HoldInteract()
  {
    if (currentInteractable != null && isInteracting)
    {
      holdTime += Time.deltaTime;
      currentInteractable.HoldInteract(holdTime);
    }
    else
    {
      // If for some reason the object we are interacting with is null, we should stop interacting
      // This could happen if an object is destroyed while we are interacting with it
      isInteracting = false;
      holdTime = 0f;
    }
  }

  private void ReleaseInteract()
  {
    if (currentInteractable != null)
    {
      currentInteractable.ReleaseInteract();
      holdTime = 0f;

      isInteracting = false;
    }
  }
}