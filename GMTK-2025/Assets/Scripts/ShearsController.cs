using FMODUnity;
using UnityEngine;

public class ShearsController : InputHandlerBase
{
    public Transform shearsOrigin;

    public float raycastDistance = 4f;
    public LayerMask sheepLayer;
    public StudioEventEmitter shearSoundEmitter;

    public void LateUpdate()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, shearsOrigin.position, 0.6f);
        transform.SetPositionAndRotation(targetPosition, shearsOrigin.rotation);
    }

    protected override void InitializeActionMap()
    {
        RegisterAction(_inputActions.Player.Shear, _ => TryShear());
    }

    private void TryShear()
    {
        // Raycast for sheep
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 1f);
        shearSoundEmitter.Play();
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, sheepLayer, QueryTriggerInteraction.Collide))
        {
            // Debug.Log("Shearable object hit: " + hit.collider.name);
            IShearable shearable = hit.collider.GetComponentInParent<IShearable>();
            if (shearable != null)
            {
                shearable.Shear();
            }
            else
            {
                // Debug.LogWarning("Hit object is not shearable: " + hit.collider.name);
            }
        }
        else
        {
            // Debug.Log("No shearable object in range.");
        }
    }
}
