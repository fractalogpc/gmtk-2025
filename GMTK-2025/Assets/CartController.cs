using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CartController : MonoBehaviour
{
    public static CartController Instance { get; private set; }

    public Transform player;

    public Collider interactableCollider;

    public bool isCartActive = false;

    public LayerMask cartLayer;
    private int sheepInCart = 0;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public GenericInteractable pullInteractable;
    private List<AdvancedSheepController> sheepInCartList = new List<AdvancedSheepController>();

    [SerializeField] private Rigidbody rigidbody;

    public int SheepInCart
    {
        get { return sheepInCart; }
        set
        {
            DisplayGameState.Instance.currentSheepCount = value;
            sheepInCart = value;
        }
    }

    public bool InZone()
    {
        if (transform.position.z <= 463.79f && transform.position.z >= 436.68f && transform.position.x <= 25.89f && transform.position.x >= -6.46f)
        {
            return true;
        }
        return false;
    }

    public void Interact()
    {
        if (isCartActive)
        {
            // Deactivate cart
            isCartActive = false;
            pullInteractable.SetInteractionName("Pull Cart");
        }
        else
        {
            // Activate cart
            isCartActive = true;
            pullInteractable.SetInteractionName("Drop Cart");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // SheepInCart = 100;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void DropCart()
    {
        isCartActive = false;
        rigidbody.transform.position = transform.position;
        rigidbody.isKinematic = false;

        StartCoroutine(FollowRigidbody());
    }

    private IEnumerator FollowRigidbody()
    {
        float elapsedTime = 0f;
        float followDuration = 5f; // Duration to follow the rigidbody

        while (elapsedTime < followDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = rigidbody.transform.position;
            yield return null;
        }

        // After following, reset the cart
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        rigidbody.isKinematic = true;
        isCartActive = false;
        foreach (var sheep in sheepInCartList)
        {
            Destroy(sheep.gameObject);
        }
        sheepInCartList.Clear();
        sheepInCart = 0;
    }

    void Update()
    {
        if (isCartActive)
        {

            if (Vector3.Distance(IgnoreY(transform.position), IgnoreY(player.position)) < 2f)
            {
                return;
            }
            // Slerp to look at the player
            Vector3 direction = IgnoreY(player.position - transform.position);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            // If the cart is close to the player, don't move it
            if (Vector3.Distance(IgnoreY(transform.position), IgnoreY(player.position)) < 5f)
            {
                return;
            }

            // Follow the the player but stay a distance away to prevent collision
            Vector3 targetPosition = player.position - direction.normalized * 5f;
            targetPosition.y = transform.position.y; // Keep the cart at the same height
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        }
    }

    private Vector3 IgnoreY(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public void TryPlaceSheep()
    {
        Debug.Log("Trying to place sheep in cart");
        if (ToolController.Instance.currentTool == ToolController.ToolType.Sheep)
        {
            AdvancedSheepController sheep = ToolController.Instance.currentlyHeldSheep;
            InventoryController.Instance.TryRemoveItem(InventoryController.Instance.SelectedSlot);
            ToolController.Instance.SetTool(ToolController.ToolType.None);

            // Place the sheep in the cart
            sheep.transform.SetParent(transform);

            // Raycast to find where to place the sheep
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, cartLayer, QueryTriggerInteraction.Collide))
            {
                sheep.transform.position = hit.point;
            }

            // Randomize the sheep's rotation on all axes
            sheep.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );

            sheep.Show();
            sheep.PutInCart(this);
            sheepInCartList.Add(sheep);
            ObjectiveSystem.Instance.CompleteObjectiveByName("PutCart");

            SheepInCart++;
        }
    }

    public void SetInteractableColliderActive(bool active)
    {
        interactableCollider.enabled = active;
    }
}
