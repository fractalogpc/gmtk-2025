using System.Collections.Generic;
using System.Threading.Tasks;
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

    private Rigidbody rb;
    private Collider lassoCollider;

    private List<SimpleSheepMover> lassoedSheep = new List<SimpleSheepMover>();

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
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lassoCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // Charging throw
        if (isChargingThrow)
        {
            throwChargeTime += Time.deltaTime;
        }

        // Reset lasso if it has been thrown for too long
        if (lassoInAir && (Vector3.Distance(transform.position, originalPosition.position) > 30f || throwChargeTime > 15f))
        {
            ResetLasso();
        }

        // Lasso is returning to hand
        if (isPullingTarget)
        {
            Vector3 targetPos = originalPosition.position;

            // Player is pulling the lasso back
            if (isRetracting)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 10f);
            }

            // Fully retracted
            if (Vector3.Distance(transform.position, targetPos) < 3f)
            {
                ResetLasso();
            }

            // Safety reset if too far
            if (Vector3.Distance(transform.position, targetPos) > 20f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 10f);
            }
        }
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
    }

    private void OnLasso(InputAction.CallbackContext ctx)
    {
        if (!canLasso) return;

        // Lasso is being held, initiate throw charge
        if (!lassoInAir)
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
                Debug.Log("Reeling in lasso");
            }
            else if (ctx.phase == InputActionPhase.Canceled)
            {
                isRetracting = false;
            }
        }
    }

    private void ThrowLasso()
    {
        Quaternion camRot = Camera.main.transform.rotation;
        Vector3 throwDir = camRot * Vector3.forward;
        Vector3 velocity = throwDir * GetLassoMagnitude(throwChargeTime);
        velocity += playerController.Motor.Velocity;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(velocity, ForceMode.VelocityChange);
        lassoCollider.enabled = true;

        throwChargeTime = 0f;
    }

    private float GetLassoMagnitude(float chargeTime)
    {
        float t = Mathf.Clamp01(chargeTime / maxLassoTime);
        return lassoCurve.Evaluate(t);
    }

    private void ResetLasso()
    {
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
    }

    public void OnLassoHit()
    {
        if (lassoInAir)
        {
            StartPullingTarget();
        }
    }

    private void StartPullingTarget()
    {
        rb.isKinematic = true;
        lassoCollider.enabled = false;
        isChargingThrow = false;
        isPullingTarget = true;

        Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 5f);
        foreach (var hit in nearbyHits)
        {
            var sheep = hit.GetComponent<SimpleSheepMover>();
            if (sheep != null)
            {
                sheep.GetLassoed(transform);
                lassoedSheep.Add(sheep);
            }
        }
    }
}
