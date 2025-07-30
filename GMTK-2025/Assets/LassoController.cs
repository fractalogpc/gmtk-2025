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

    private bool isHeld = true;
    private Rigidbody rb;
    private Collider lassoCollider;

    private bool lassoIsOut = false;
    private bool validPress = true;
    private bool isHolding = false;
    private bool startCounting = false;

    private float timeHeld = 0f;

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
        if (startCounting)
        {
            timeHeld += Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        if (isHeld)
        {
            transform.rotation = originalPosition.rotation;

            if (startCounting)
            {
                // Pull back the lasso based on time held
                float pullbackPercentage = Mathf.Clamp(timeHeld / maxLassoTime, 0f, 1f);
                float pullbackDistance = pullbackPercentage * 0.5f;
                Vector3 pullbackPosition = originalPosition.position + Camera.main.transform.forward * -pullbackDistance;
                transform.position = pullbackPosition;
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
        if (!validPress) return;

        if (!lassoIsOut)
        {
            // Start pulling back when button is pressed
            if (ctx.phase == InputActionPhase.Performed)
            {
                // Debug.Log("Pulling back");
                startCounting = true;
                isHolding = true;
            }
            // Throw lasso when button is released
            else if (ctx.phase == InputActionPhase.Canceled && isHolding)
            {
                // Debug.Log("Throwing lasso");
                startCounting = false;
                ThrowLasso();
                timeHeld = 0f;
                lassoIsOut = true;
                isHeld = false;
                validPress = true;
                isHolding = false;
            }
        }
        else
        {
            // Reset lasso when button is pressed again
            if (ctx.phase == InputActionPhase.Performed)
            {
                // Debug.Log("Resetting");
                ResetLasso();
                lassoIsOut = false;
                validPress = true;
            }
        }
    }

    private void ThrowLasso()
    {
        Quaternion cameraRotation = Camera.main.transform.rotation;
        Vector3 throwDirection = cameraRotation * Vector3.forward;

        Vector3 lassoVelocity = throwDirection * GetLassoMagnitude(timeHeld); // Adjust speed as necessary

        Vector3 playerVelocity = playerController.Motor.Velocity;

        lassoVelocity += playerVelocity; // Add player's velocity to the lasso's velocity

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero; // Reset any previous velocity
        rb.AddForce(lassoVelocity, ForceMode.VelocityChange);
        lassoCollider.enabled = true;

    }

    private float GetLassoMagnitude(float time)
    {
        float t = Mathf.Clamp(time / maxLassoTime, 0f, 1f);
        return lassoCurve.Evaluate(t);
    }

    private void ResetLasso()
    {
        isHeld = true;
        canLasso = true;
        rb.isKinematic = true;
        lassoCollider.enabled = false;
    }

    public void OnLassoHit()
    {
        if (lassoIsOut)
        {
            InitiateLasso();
        }
    }

    private void InitiateLasso()
    {
        ResetLasso();
        validPress = false; // Prevent further lasso actions until reset
    }

}
