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
    if (RaycastForInteractable())
    {
      if (!isHovering)
      {
        currentInteractable.OnHoverEnter();
        isHovering = true;
      }
    }
    else
    {
      if (isHovering)
      {
        previousInteractable.OnHoverExit();
        isHovering = false;
      }
    }

    if (isInteracting)
    {
      HoldInteract();
    }


  }

  private bool RaycastForInteractable()
  {
    Ray ray = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
    if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer, QueryTriggerInteraction.Collide))
    {
      IInteractable interactable = hit.collider.GetComponent<IInteractable>();
      if (interactable != null)
      {
        currentInteractable = interactable;
        currentInteractableObject = hit.collider.gameObject;
        return true;
      }
    }

    currentInteractable = null;
    currentInteractableObject = null;

    previousInteractable = currentInteractable;
    return false;
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