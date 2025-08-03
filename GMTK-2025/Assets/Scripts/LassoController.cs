
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FMODUnity;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class LassoController : InputHandlerBase
{
    public bool canLasso = true;
    public Transform originalPosition;
    public PlayerController playerController;
    public AnimationCurve lassoCurve;
    public float maxLassoTime = 2f;
    public StudioEventEmitter chargeSoundEmitter;
    public StudioEventEmitter throwSoundEmitter;
    public StudioEventEmitter pullSoundEmitter;
    public StudioEventEmitter breakSoundEmitter;

    public LayerMask sheepMask;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Collider lassoCollider;

    public GameObject heldLasso;
    public Renderer tableLassoRenderer;

    public LassoVisualController visualController;

    public List<AdvancedSheepController> lassoedSheep = new List<AdvancedSheepController>();

    // Lasso State Booleans
    private bool lassoHeldInHand = true;         // Lasso is in player's hand
    private bool isChargingThrow = false;        // Holding button to charge throw
    private bool lassoInAir = false;             // Lasso has been thrown
    private bool isRetracting = false;           // Lasso is returning or reeling in
    private bool isPullingTarget = false;        // Lasso has hit something and is pulling

    private float throwChargeTime = 0f;

    protected override void InitializeActionMap()
    {
        RegisterActionComplexCancel(_inputActions.Player.Lasso, ctx => OnLasso(ctx), ctx => OnLasso(ctx));

        RegisterAction(_inputActions.Player.Drop, _ => ResetLasso());
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lassoCollider = GetComponent<Collider>();
    }

    public override void Start()
    {
        base.Start();
        ResetLasso();
    }

    void LateUpdate()
    {

        if (lassoHeldInHand)
        {
            transform.rotation = originalPosition.rotation;

            if (isChargingThrow)
            {
                float percentCharged = Mathf.Clamp01(throwChargeTime / maxLassoTime);
                float pullbackDist = percentCharged * 0.5f;
                transform.position = originalPosition.position + Camera.main.transform.forward * -pullbackDist;
            }
            else
            {
                transform.position = originalPosition.position;
            }
        }

        // Charging throw
        if (isChargingThrow)
        {
            throwChargeTime += Time.deltaTime;
        }

        // Reset lasso if it has been thrown for too long
        if (lassoInAir && (Vector3.Distance(transform.position, originalPosition.position) > 60f || throwChargeTime > 15f))
        {
            ResetLasso();
        }

        // Lasso is returning to hand
        if (isPullingTarget)
        {
            Vector3 targetPos = originalPosition.position;

            if (lassoedSheep.Count == 0) isRetracting = true;

            // Player is pulling the lasso back
            if (isRetracting)
            {
                transform.position = GetGroundHeight(Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 15f));
            }

            // Fully retracted
            if (Vector3.Distance(transform.position, targetPos) < 2f && lassoedSheep.Count == 0)
            {
                ResetLasso();
            }

            // Safety reset if too far
            if (Vector3.Distance(transform.position, targetPos) > 70f)
            {
                transform.position = GetGroundHeight(Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 10f));
            }
        }

        if (StartedChargingThrow())
        {
            // print("started charging throw");
            chargeSoundEmitter.Play();
        }
        if (StoppedChargingThrow())
        {
            // print("stopped charging throw");
            chargeSoundEmitter.Stop();
        }
        if (StartedPulling())
        {
            // print("started pulling");
            pullSoundEmitter.Play();
        }
        if (StoppedPulling())
        {
            // print("stopped pulling");
            pullSoundEmitter.Stop();
        }

        isRetractingLastFrame = isRetracting;
        isChargingThrowLastFrame = isChargingThrow;
    }
    private bool isRetractingLastFrame = false;
    private bool StartedPulling()
    {
        if (!isRetractingLastFrame && isRetracting)
        {
            return true;
        }

        return false;
    }
    private bool StoppedPulling()
    {
        if (isRetractingLastFrame && !isRetracting)
        {
            return true;
        }

        return false;
    }

    private bool isChargingThrowLastFrame = false;
    private bool StartedChargingThrow()
    {
        if (!isChargingThrowLastFrame && isChargingThrow)
        {
            isChargingThrowLastFrame = isChargingThrow;
            return true;
        }
        return false;
    }
    private bool StoppedChargingThrow()
    {
        if (isChargingThrowLastFrame && !isChargingThrow)
        {
            return true;
        }

        return false;
    }


    private void OnLasso(InputAction.CallbackContext ctx)
    {
        if (!canLasso) return;

        // Lasso is being held, initiate throw charge
        if (!lassoInAir && !isPullingTarget)
        {
            if (ctx.phase == InputActionPhase.Performed)
            {
                isChargingThrow = true;
            }
            else if (ctx.phase == InputActionPhase.Canceled && isChargingThrow)
            {
                isChargingThrow = false;
                ThrowLasso();
                lassoInAir = true;
                lassoHeldInHand = false;
            }
        }
        else // Lasso is out; handle retraction
        {
            if (ctx.phase == InputActionPhase.Performed)
            {
                isRetracting = true;
                visualController.StartPulling(true);
            }
            else if (ctx.phase == InputActionPhase.Canceled)
            {
                isRetracting = false;
                visualController.StartPulling(false);
            }
        }
    }

    // Lasso leaves the player's hand and is thrown
    private void ThrowLasso()
    {
        InventoryController.Instance.SetClickPromptText("Pull");
        InventoryController.Instance.SetDropPromptActive(true);
        InventoryController.Instance.SetDropPromptText("Reset");

        Quaternion camRot = Camera.main.transform.rotation;
        Vector3 throwDir = camRot * Vector3.forward;
        Vector3 velocity = throwDir * GetLassoMagnitude(throwChargeTime);
        velocity += playerController.Motor.Velocity;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(velocity, ForceMode.VelocityChange);
        lassoCollider.enabled = true;

        throwChargeTime = 0f;

        heldLasso.SetActive(false);
        visualController.EnableVisual();
        throwSoundEmitter.Play();
    }

    private float GetLassoMagnitude(float chargeTime)
    {
        float t = Mathf.Clamp01(chargeTime / maxLassoTime);
        return lassoCurve.Evaluate(t);
    }

    public List<AdvancedSheepController> ReleaseSheep(int count, Pen.SubPen targetPen)
    {
        if (count != lassoedSheep.Count)
        {
            Debug.LogWarning("Missmatching counts, something is wrong!");
        }

        for (int i = 0; i < count; i++)
        {
            lassoedSheep[i].SendToPen(targetPen);
        }

        List<AdvancedSheepController> releasedSheep = lassoedSheep.GetRange(0, count);

        List<Transform> sheepTransforms = new List<Transform>();
        foreach (var sheep in releasedSheep)
        {
            sheepTransforms.Add(sheep.transform);
        }
        visualController.lassoLoopController.ReleasePoints(sheepTransforms.ToArray());

        lassoedSheep.RemoveRange(0, count);

        return releasedSheep; // Return the released sheep
    }

    public void ResetLasso()
    {
        InventoryController.Instance.SetClickPromptText("Throw");

        InventoryController.Instance.SetDropPromptActive(false);
        InventoryController.Instance.SetDropPromptText("Drop");

        heldLasso.SetActive(true);
        visualController.DisableVisual();

        // Debug.Log("Reset lasso");
        // Debug.Log(visualController == null ? "visualController is null" : "visualController is assigned");
        // visualController.EnableVisual();
        lassoHeldInHand = true;
        lassoInAir = false;
        isPullingTarget = false;
        isRetracting = false;
        throwChargeTime = 0f;

        rb.isKinematic = true;
        lassoCollider.enabled = false;
        canLasso = true;

        foreach (var sheep in lassoedSheep)
        {
            sheep.Reset(); // Stop following the lasso
        }
        lassoedSheep.Clear();

        SheepReception.Instance.currentSheepCount = 0;

    }

    public void OnLassoHit()
    {
        if (lassoInAir)
        {
            visualController.HitGround();
            lassoInAir = false;
            isPullingTarget = true;
            StartPullingTarget();
        }
        throwSoundEmitter.Stop();
    }

    private void StartPullingTarget()
    {
        rb.isKinematic = true;
        lassoCollider.enabled = false;
        isChargingThrow = false;
        isPullingTarget = true;

        // transform.position = GetGroundHeight(transform.position);
        transform.position = GetGroundHeight(transform.position);

        float range = visualController.lassoLoopController.radius;
        Collider[] nearbyHits = Physics.OverlapSphere(transform.position, range);
        foreach (var hit in nearbyHits)
        {
            var sheep = hit.GetComponent<AdvancedSheepController>();
            if (sheep != null)
            {
                sheep.GetLassoed(transform, playerController.transform, this);
                lassoedSheep.Add(sheep);
            }
        }

        SheepReception.Instance.currentSheepCount = lassoedSheep.Count;

        visualController.LassoedSheep(lassoedSheep.ToArray());
    }

    public void RemoveSheep(AdvancedSheepController sheep)
    {

        StartCoroutine(DelayRecalculateAmount(0.1f, sheep)); // This is stupid, i know
    }

    private IEnumerator DelayRecalculateAmount(float time, AdvancedSheepController sheep)
    {
        yield return new WaitForSeconds(time);
        if (lassoedSheep.Contains(sheep))
        {
            lassoedSheep.Remove(sheep);
        }
        else
        {
            Debug.LogWarning("Sheep not found in lassoedSheep list!");
        }

        SheepReception.Instance.currentSheepCount = lassoedSheep.Count;
    }

    private Vector3 GetGroundHeight(Vector3 position)
    {
        Vector3 newPosition = position;
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f, layerMask: groundMask))
        {
            newPosition.y = hit.point.y;
            return newPosition;
        }
        return position; // Default to current height if no ground found
    }

    public void DeselectLasso()
    {
        lassoHeldInHand = true;
        isChargingThrow = false;
        lassoInAir = false;
        isRetracting = false;
        isPullingTarget = false;
        throwChargeTime = 0f;

        transform.SetPositionAndRotation(originalPosition.position, originalPosition.rotation);
        rb.isKinematic = true;
        lassoCollider.enabled = false;

        if (lassoedSheep.Count > 0)
        {
            foreach (var sheep in lassoedSheep)
            {
                sheep.Reset(); // Stop following the lasso
            }
            lassoedSheep.Clear();
        }

        SheepReception.Instance.currentSheepCount = 0;
        throwSoundEmitter.Stop();
        pullSoundEmitter.Stop();
        chargeSoundEmitter.Stop();
        GetComponent<LassoVisualController>().DisableVisual();
    }

    public void Upgrade1()
    {
        heldLasso.GetComponent<Renderer>().material = UpgradeManager.Instance.upgrade1Material;
        tableLassoRenderer.material = UpgradeManager.Instance.upgrade1Material;

        visualController.Upgrade1();
    }

    public void Upgrade2()
    {
        heldLasso.GetComponent<Renderer>().material = UpgradeManager.Instance.upgrade2Material;
        tableLassoRenderer.material = UpgradeManager.Instance.upgrade2Material;

        visualController.Upgrade2();
    }

    public void Upgrade3()
    {
        heldLasso.GetComponent<Renderer>().material = UpgradeManager.Instance.upgrade3Material;
        tableLassoRenderer.material = UpgradeManager.Instance.upgrade3Material;

        visualController.Upgrade3();
    }
}
