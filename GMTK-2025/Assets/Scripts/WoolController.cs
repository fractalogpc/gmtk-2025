using UnityEngine;

public class WoolController : InputHandlerBase
{
    public Transform woolOrigin;

    private Renderer woolRenderer;

    public GameObject woolPrefab;

    void Awake()
    {
        woolRenderer = GetComponentInChildren<Renderer>();
    }

    public void LateUpdate()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, woolOrigin.position, 0.4f);
        transform.SetPositionAndRotation(targetPosition, woolOrigin.rotation);
    }

    public void SetMaterial(Material material)
    {
        woolRenderer.material = material;
    }

    protected override void InitializeActionMap()
    {
        RegisterAction(_inputActions.Player.Drop, _ => TryDropWool());
    }

    private void TryDropWool()
    {
        if (!InventoryController.Instance.IsHoldingObject(InventoryController.ItemType.Wool)) return;

        InventoryController.Instance.TryRemoveItem();

        GameObject woolInstance = Instantiate(woolPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
        Renderer renderer = woolInstance.GetComponentInChildren<Renderer>();

        InventoryController.WoolData woolData = InventoryController.Instance.GetWoolData();

        renderer.material = InventoryController.Instance.woolMaterials[woolData.ColorIndex]; // Use the first wool material as a base
        woolInstance.GetComponentsInChildren<Pickuppable>()[0].woolSize = woolData.Size;
        // Debug.Log("Wool size: " + woolSize);
        woolInstance.GetComponentsInChildren<Pickuppable>()[0].woolColorIdx = woolData.ColorIndex;
        // Vector3 initialVelocity = new Vector3(UnityEngine.Random.Range(-.2f, 0.2f), 0.5f, UnityEngine.Random.Range(-0.2f, 0.2f)).normalized * 5f;
        // woolInstance.GetComponent<Rigidbody>().AddForce(initialVelocity, ForceMode.Impulse);
        // woolPopSoundEmitter.Play();
    }
}
