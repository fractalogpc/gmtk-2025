using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
  public class PlayerCamera : InputHandlerBase
  {
    public bool canLook = true;
    [SerializeField] private Transform _playerTransform;
    // [SerializeField] private PlayerViewBob _viewBob;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private float _minYRotCutscene = -45f;
    [SerializeField] private float _maxYRotCutscene = 45f;
    public float sensitivity = 1f;

    [HideInInspector] public Quaternion PlayerYLookQuaternion = Quaternion.identity;

    [HideInInspector] public float CameraXRotation = 0;
    private Vector2 _mouseInput;
    private bool _inCutscene = false;

    private float _aggregateYRotation = 0;
    private float _countingYCutsceneRot = 0;

    // Maximum vertical look angle (just below 90°)
    private const float MaxVerticalAngle = 89f;

    protected override void InitializeActionMap()
    {
      _actionMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>();

      RegisterAction(_inputActions.Player.Look, ctx => _mouseInput = ctx.ReadValue<Vector2>() * sensitivity, () => _mouseInput = Vector2.zero);
    }

    private void Awake()
    {
      // Fetch correct Y rotation from transform
      PlayerYLookQuaternion = Quaternion.Euler(0, _playerTransform.rotation.eulerAngles.y, 0);
    }

    // private void Update()
    // {
    //   if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
    //   {
    //     canLook = false;
    //   }
    //   else
    //   {
    //     canLook = true;
    //   }
    // }

    // public void EnterCutscene()
    // {
    //   _inCutscene = true;
    // }

    // public void ExitCutscene()
    // {
    //   _inCutscene = false;
    // }

    // Ok, this is really weird. Basically the PlayerController updates its Y rotation on a FixedUpdate loop, but this script updates the X rotation on a Update Loop.
    // In order to solve this, I store a local quaternion for the Player rotation that I update here, then fetch it when I need to update the PlayerController rotation.
    // This works, however it means that you can't rotate the PlayerController internally, instead you have to rotate it here.
    private void PlayerYLook()
    {
      PlayerYLookQuaternion *= Quaternion.AngleAxis(_mouseInput.x, Vector3.up);
      _aggregateYRotation += _mouseInput.x;
      _countingYCutsceneRot += _mouseInput.x;
      _countingYCutsceneRot = Mathf.Clamp(_countingYCutsceneRot, _minYRotCutscene, _maxYRotCutscene);
    }

    private void CameraXLook()
    {
      CameraXRotation -= _mouseInput.y;
      if (CameraXRotation > 90) CameraXRotation = 90;
      if (CameraXRotation < -90) CameraXRotation = -90;

      CameraXRotation = Mathf.Clamp(CameraXRotation, -MaxVerticalAngle, MaxVerticalAngle);
    }

    // Remnents from previous implementation
    // private void UpdateViewmodel()
    // {
    //   Vector2 cameraTargetRot = new Vector2(CameraXRotation, _aggregateYRotation);

    //   _viewmodelTargetRotation = Vector2.Lerp(_viewmodelTargetRotation, cameraTargetRot, Time.deltaTime * _viewmodelFollowSpeed);
    //   // Clamp viewmodel offset
    //   Vector2 diff = _viewmodelTargetRotation - cameraTargetRot;
    //   diff.x = Mathf.Clamp(diff.x, -_viewmodelMaxXOffset, _viewmodelMaxXOffset);
    //   diff.y = Mathf.Clamp(diff.y, -_viewmodelMaxYOffset, _viewmodelMaxYOffset);

    //   _viewmodelTransform.localRotation = Quaternion.Euler(diff.x, diff.y, 0);
    // }

    private void LateUpdate()
    {
      if (!canLook) return;

      PlayerYLook();

      CameraXLook();
      Quaternion newRotation = Quaternion.Euler(CameraXRotation, _playerTransform.rotation.eulerAngles.y, 0);

      if (_inCutscene)
      {
        newRotation = Quaternion.Euler(0, _countingYCutsceneRot, 0);
        CameraTransform.localRotation = newRotation;
        return;
      }

      // UpdateViewmodel();

      CameraTransform.SetPositionAndRotation(_cameraTarget.position, newRotation);
    }

    public void ResetRotation(Quaternion rotation)
    {
      CameraXRotation = rotation.eulerAngles.x;
      _aggregateYRotation = rotation.eulerAngles.y;
      PlayerYLookQuaternion = Quaternion.Euler(0, _aggregateYRotation, 0);
    }
  }
}
