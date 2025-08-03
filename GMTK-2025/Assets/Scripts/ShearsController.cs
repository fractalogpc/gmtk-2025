using FMODUnity;
using UnityEngine;

public class ShearsController : InputHandlerBase
{
    public Transform shearsOrigin;

    public float raycastDistance = 3f;
    public LayerMask sheepLayer;
    public StudioEventEmitter shearSoundEmitter;

    [System.Serializable]
    public class GameObjectGroup
    {
        public GameObject[] objects;
    }

    public GameObjectGroup[] objects;

    bool isHolding = false;

    public void LateUpdate()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, shearsOrigin.position, 0.6f);
        transform.SetPositionAndRotation(targetPosition, shearsOrigin.rotation);
    }

    protected override void InitializeActionMap()
    {
        RegisterAction(_inputActions.Player.Shear, _ => TryShear());
        RegisterAction(_inputActions.Player.Shear, _ => { if (upgrade3) isHolding = true; }, () => { isHolding = false; });
    }

    private void Update()
    {
        if (isHolding)
        {
            TryShear();
        }
    }

    private void TryShear()
    {
        if (gameObject.activeInHierarchy == false) return;
        // Raycast for sheep
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 1f);
        shearSoundEmitter.Play();
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, sheepLayer, QueryTriggerInteraction.Collide))
        {
            // Debug.Log("Shearable object hit: " + hit.collider.name);
            IShearable shearable = hit.collider.GetComponentInParent<IShearable>();
            if (shearable != null && shearable.CanBeSheared())
            {

                bool doubleShear = false;

                float randValue = Random.Range(0f, 1f);
                if (upgrade2)
                {
                    if (randValue < 0.2f)
                    {
                        doubleShear = true;
                    }
                }
                else if (upgrade3)
                {
                    if (randValue < 0.5f)
                    {
                        doubleShear = true;
                    }
                }

                shearable.Shear(doubleShear);
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

    bool upgrade1 = false;
    bool upgrade2 = false;
    bool upgrade3 = false;

    private void OnDisable()
    {
        Debug.Log("Disabling ShearsController");
        isHolding = false;
    }

    public void Upgrade1()
    {
        foreach (var obj in objects[0].objects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in objects[1].objects)
        {
            obj.SetActive(true);
        }

        upgrade1 = true;
        raycastDistance = 5f;
    }

    public void Upgrade2()
    {
        foreach (var obj in objects[1].objects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in objects[2].objects)
        {
            obj.SetActive(true);
        }

        upgrade2 = true;
    }

    public void Upgrade3()
    {
        foreach (var obj in objects[2].objects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in objects[3].objects)
        {
            obj.SetActive(true);
        }

        upgrade3 = true;
        raycastDistance = 6f;

        shearSoundEmitter.SetParameter("Volume", 0.4f);
    }

}
