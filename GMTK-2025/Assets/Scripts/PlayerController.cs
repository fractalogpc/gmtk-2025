using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
  public enum CharacterState
  {
    Default,
    Water,
    Noclip
  }

  public class PlayerController : InputHandlerBase, ICharacterController
  {

    public bool isEnabled;

    public KinematicCharacterMotor Motor;
    public PlayerCamera PlayerCamera;

    #region Variables

    [Header("Stable Movement")]
    public float MaxStableCrouchSpeed = 2f;
    public float MaxStableMoveSpeed = 3.5f;
    public float MaxStableSprintSpeed = 6f;
    public float StableMovementSharpness = 15f;

    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 10f;
    public float AirAccelerationSpeed = 30f;
    public float Drag = 0f;

    [Header("Jumping")]
    public bool AllowJumpingWhenSliding = false;
    public bool AllowHoldingJump = false;
    public float JumpUpSpeed = 10f;
    public float JumpScalableForwardSpeed = 0f;
    public float JumpPreGroundingGraceTime = 0.1f;
    public float JumpPostGroundingGraceTime = 0.1f;

    [Header("Noclip")]
    public float NoclipMoveSpeed = 10f;
    public float NoclipRunSpeed = 20f;

    [Header("Misc")]
    public List<Collider> IgnoredColliders = new List<Collider>();
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public float CrouchedCapsuleHeight = 1f;

    public AnimationCurve FallDamageCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    public float _previousFrameFallSpeed = 0f;

    public LayerMask WaterLayer;
    public float WaterOffset = 0f;
    public float Bouyancy = 10f;
    public float WaterDrag = 0.5f;

    public CharacterState CurrentCharacterState { get; private set; }
    // field: at the beginning allows for properties to be serialized
    [field: SerializeField] public Vector3 MoveInputVector { get; private set; }

    [Header("Events")]
    // public UnityEvent<SoundEvent> OnStep;

    private Collider[] _probedColliders = new Collider[8];
    private Collider[] _testWaterColliders = new Collider[5];
    private RaycastHit[] _probedHits = new RaycastHit[8];
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private Vector3 _internalVelocityAdd = Vector3.zero;
    private bool _shouldBeCrouching = false;
    private bool _isCrouching = false;

    private float _currentMaxSpeed = 0f;


    // Input variables
    private Vector2 _moveInput;
    private bool _jumpInput;
    private bool _crouchInput;
    private bool _sprintInput;

    #endregion

    #region Input

    protected override void InitializeActionMap()
    {
      _actionMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>();

      RegisterAction(_inputActions.Player.Move, ctx => _moveInput = ctx.ReadValue<Vector2>(), () => _moveInput = Vector2.zero);
      RegisterAction(_inputActions.Player.Sprint, _ => _sprintInput = true, () => _sprintInput = false);
      RegisterAction(_inputActions.Player.Jump, _ => _jumpInput = true, () => _jumpInput = false);

      RegisterAction(_inputActions.Player.Noclip, _ => ToggleNoclip(), null);
    }

    #endregion

    private void ToggleNoclip()
    {
      if (CurrentCharacterState == CharacterState.Noclip)
      {
        TransitionToState(CharacterState.Default);
      }
      else
      {
        TransitionToState(CharacterState.Noclip);
      }
    }

    private void Awake()
    {
      TransitionToState(CharacterState.Default);

      Motor.CharacterController = this;
    }

    public void InitializeStart()
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }

    public void Disable()
    {
      isEnabled = false;
      Motor.enabled = false;
      GetComponent<Collider>().enabled = false;
    }

    public void Enable()
    {
      isEnabled = true;
      Motor.enabled = true;
      GetComponent<Collider>().enabled = true;
    }

    private void Update()
    {
      // if (Input.GetKeyDown(KeyCode.P))
      // {
      //   Time.timeScale = 5f;
      // }
      // if (Input.GetKeyUp(KeyCode.P))
      // {
      //   Time.timeScale = 1f;
      // }

      if (!isEnabled) return;

      if (Motor.Velocity.y != 0)
        _previousFrameFallSpeed = Motor.Velocity.y;

      HandleCharacterInput();

      if (CurrentCharacterState == CharacterState.Noclip)
      {
        HandleNoclipMovement();
      }

      // OnStep?.Invoke(new SoundEvent(transform.position, 100f, "Footstep"));
    }

    private void HandleCharacterInput()
    {
      MoveInputVector = transform.rotation * new Vector3(_moveInput.x, 0f, _moveInput.y);
      switch (CurrentCharacterState)
      {
        case CharacterState.Default:

          if (_crouchInput)
          {
            _currentMaxSpeed = MaxStableCrouchSpeed;
          }
          else if (_sprintInput)
          {
            _currentMaxSpeed = MaxStableSprintSpeed;
          }
          else
          {
            _currentMaxSpeed = MaxStableMoveSpeed;
          }

          if (_jumpInput)
          {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
          }

          if (_crouchInput)
          {
            _shouldBeCrouching = true;

            if (!_isCrouching)
            {
              _isCrouching = true;
              Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
              MeshRoot.localScale = new Vector3(1, 0.5f, 1);
            }
          }
          else if (_isCrouching)
          {
            _shouldBeCrouching = false;
            // Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
            // MeshRoot.localScale = new Vector3(1, 1, 1);
          }

          break;
        case CharacterState.Water:
          break;
        case CharacterState.Noclip:
          if (_sprintInput)
          {
            _currentMaxSpeed = NoclipRunSpeed;
          }
          else
          {
            _currentMaxSpeed = NoclipMoveSpeed;
          }

          MoveInputVector += Vector3.up * (_jumpInput ? 1 : 0) + Vector3.down * (_crouchInput ? 1 : 0);
          MoveInputVector.Normalize();
          break;
      }
    }

    private void HandleNoclipMovement()
    {

      // Orientate the moveDirection vector with the Camera's Y axis
      // Vector3 moveDirection = Quaternion.Euler(0, PlayerCamera.PlayerYLookQuaternion.eulerAngles.y, 0) * MoveInputVector; // TODO: Make this work

      float maxSpeed = _currentMaxSpeed;
      // If player holds two opposing directions, speed up currentMaxSpeed
      if (_jumpInput && _crouchInput)
      {
        maxSpeed = _currentMaxSpeed * 20;
      }

      Vector3 move = MoveInputVector * maxSpeed * Time.deltaTime;
      transform.position += move;

      transform.rotation = PlayerCamera.PlayerYLookQuaternion;
    }


    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    /// <summary>
    /// This should only be called from PlayerCamera script.
    /// </summary>
    /// <param name="rotation"></param>
    public void SetRotation(Quaternion rotation)
    {
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
      switch (CurrentCharacterState)
      {
        case CharacterState.Default:
          currentRotation = PlayerCamera.PlayerYLookQuaternion;
          break;
        case CharacterState.Water:
          break;
      }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
      if (resetVelocity)
      {
        currentVelocity = Vector3.zero;
        resetVelocity = false;
      }

      switch (CurrentCharacterState)
      {
        case CharacterState.Default:
          {
            // Ground movement
            if (Motor.GroundingStatus.IsStableOnGround)
            {
              float currentVelocityMagnitude = currentVelocity.magnitude;

              Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

              // Reorient velocity on slope
              currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

              // Calculate target velocity
              Vector3 inputRight = Vector3.Cross(MoveInputVector, Motor.CharacterUp);
              Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * MoveInputVector.magnitude;
              Vector3 targetMovementVelocity = reorientedInput * _currentMaxSpeed;

              // Smooth movement Velocity
              currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            // Air movement
            else
            {
              // Add move input
              if (MoveInputVector.sqrMagnitude > 0f)
              {
                Vector3 addedVelocity = MoveInputVector * AirAccelerationSpeed * deltaTime;

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                {
                  // clamp addedVel to make total vel not exceed max vel on inputs plane
                  Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                  addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                  // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                  if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                  {
                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                  }
                }

                // Prevent air-climbing sloped walls
                if (Motor.GroundingStatus.FoundAnyGround)
                {
                  if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                  {
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                  }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
              }

              // Gravity
              currentVelocity += Gravity * deltaTime;

              // Drag
              currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }

            // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested)
            {
              // See if we actually are allowed to jump
              if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
              {
                // Calculate jump direction before ungrounding
                Vector3 jumpDirection = Motor.CharacterUp;
                if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                {
                  jumpDirection = Motor.GroundingStatus.GroundNormal;
                }

                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                Motor.ForceUnground();

                // Add to the return velocity and reset jump state
                currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                currentVelocity += (MoveInputVector * JumpScalableForwardSpeed);
                _jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;

                // Disable jumping if required
                if (!AllowHoldingJump)
                {
                  _jumpInput = false;
                }
              }
            }

            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
              currentVelocity += _internalVelocityAdd;
              _internalVelocityAdd = Vector3.zero;
            }
            break;
          }
        case CharacterState.Water:
          {
            break;
          }
      }

      // Test for water
      if (CheckIsInWater())
      {
        // If in water, get the percentage of the character in water
        if (!Physics.Raycast(Motor.TransientPosition + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, WaterLayer)) return;
        float waterHeight = hit.point.y + WaterOffset;
        float characterHeightWorldposition = Motor.InitialSimulationPosition.y;
        float submergedPercentage = Mathf.Clamp01((waterHeight - characterHeightWorldposition) / (Motor.Capsule.height));

        // Apply bouyancy to the character
        currentVelocity += Vector3.up * Bouyancy * submergedPercentage * Time.deltaTime;
        // Apply drag to the character
        currentVelocity *= 1 - (submergedPercentage * WaterDrag * Time.deltaTime);

        // Tell the player it isn't on stable ground
        if (Motor.GroundingStatus.IsStableOnGround)
        {
          Motor.ForceUnground();
        }
      }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {
      switch (CurrentCharacterState)
      {
        case CharacterState.Default:
          {
            // Handle jump-related values
            {
              // Handle jumping pre-ground grace period
              if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
              {
                _jumpRequested = false;
              }

              if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
              {
                // If we're on a ground surface, reset jumping values
                if (!_jumpedThisFrame)
                {
                  _jumpConsumed = false;
                }
                _timeSinceLastAbleToJump = 0f;
              }
              else
              {
                // Keep track of time since we were last able to jump (for grace period)
                _timeSinceLastAbleToJump += deltaTime;
              }
            }

            // Handle uncrouching
            if (_isCrouching && !_shouldBeCrouching)
            {
              // Do an overlap test with the character's standing height to see if there are any obstructions
              Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
              if (Motor.CharacterOverlap(
                  Motor.TransientPosition,
                  Motor.TransientRotation,
                  _probedColliders,
                  Motor.CollidableLayers,
                  QueryTriggerInteraction.Ignore) > 0)
              {
                // If obstructions, just stick to crouching dimensions
                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
              }
              else
              {
                // If no obstructions, uncrouch
                MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                _isCrouching = false;
              }
            }
            break;
          }
      }
    }

    bool CheckIsInWater()
    {
      Vector3 position = Motor.TransientPosition;
      Quaternion rotation = Motor.TransientRotation;
      if (Motor.CharacterOverlap(
        position, rotation,
        _testWaterColliders,
        WaterLayer,
        QueryTriggerInteraction.Collide
      ) > 0)
      {
        return true;
      }
      return false;
    }

    public void PostGroundingUpdate(float deltaTime)
    {
      // Handle landing and leaving ground
      if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
      {
        OnLanded();
      }
      else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
      {
        OnLeaveStableGround();
      }
    }


    public bool IsColliderValidForCollisions(Collider coll)
    {
      if (IgnoredColliders.Count == 0)
      {
        return true;
      }

      if (IgnoredColliders.Contains(coll))
      {
        return false;
      }

      return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    protected void OnLanded()
    {
      
    }

    protected void OnLeaveStableGround()
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void AddVelocity(Vector3 velocity)
    {
      switch (CurrentCharacterState)
      {
        case CharacterState.Default:
          {
            _internalVelocityAdd += velocity;
            break;
          }
      }
    }

    public void TransitionToState(CharacterState newState)
    {
      CharacterState oldState = CurrentCharacterState;
      OnStateExit(oldState, newState);
      CurrentCharacterState = newState;
      OnStateEnter(newState, oldState);
    }

    private void OnStateEnter(CharacterState newState, CharacterState previousState)
    {
      switch (newState)
      {
        case CharacterState.Default:
          break;
        case CharacterState.Water:
          break;
        case CharacterState.Noclip:
          Motor.enabled = false;
          GetComponent<Collider>().enabled = false;
          break;
      }
    }

    private bool resetVelocity = false;
    private void OnStateExit(CharacterState previousState, CharacterState newState)
    {
      switch (previousState)
      {
        case CharacterState.Default:
          break;
        case CharacterState.Water:
          break;
        case CharacterState.Noclip:
          // If leaving noclip and holding up and down, keep the player position
          if (!(_jumpInput && _crouchInput))
          {
            Motor.SetPosition(transform.position);
          }

          Motor.enabled = true;
          GetComponent<Collider>().enabled = true;

          // Reset the player velocity
          resetVelocity = true;
          break;
      }
    }

    public void SetPositionAndRotation(Transform setTransform) {
      Motor.SetPositionAndRotation(setTransform.position, setTransform.rotation);
      GetComponentInChildren<PlayerCamera>().ResetRotation(setTransform.rotation);
    }

    public void SetPosition(Vector3 position) {
      Motor.SetPosition(position);
    }
  }
}
