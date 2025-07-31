using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Abstract base class for handling input actions.
/// </summary>
public abstract class InputHandlerBase : MonoBehaviour
{
  protected PlayerInputActions _inputActions;
  protected bool _enabledOnce = false;

  // Dictionary to store input actions and their callbacks
  protected Dictionary<InputAction, Action<InputAction.CallbackContext>> _actionMap = new();

  public virtual void Start()
  {
    _inputActions = InputReader.Instance.InputActions;
    InitializeActionMap();

    // Subscribe all actions in the dictionary
    foreach (var action in _actionMap)
    {
      action.Key.performed += action.Value;
    }

    _enabledOnce = true;
  }

  private void OnEnable()
  {
    if (_enabledOnce)
    {
      // Subscribe all actions in the dictionary
      foreach (var action in _actionMap)
      {
        action.Key.performed += action.Value;
      }
    }
  }

  private void OnDisable()
  {
    // Unsubscribe all actions in the dictionary
    foreach (var action in _actionMap)
    {
      action.Key.performed -= action.Value;
    }
  }

  /// <summary>
  /// Derived classes must implement this method to define which input actions to handle
  /// and map each action to its respective callback. This function is called in OnEnableFunction.
  /// </summary>
  protected abstract void InitializeActionMap();

  /// <summary>
  /// Registers an input action with its associated callback functions for performing and canceling the action.
  /// This method maps the input action to a callback, allowing easy management and unsubscription.
  /// </summary>
  /// <param name="action">The input action to register (e.g., jump, move).</param>
  /// <param name="performCallback">The callback function to invoke when the action is performed (e.g., button press).</param>
  /// <param name="cancelCallback">Optional callback for when the action is canceled (e.g., button release).</param>
  protected void RegisterAction(InputAction action, Action<InputAction.CallbackContext> performCallback, Action cancelCallback = null)
  {
    action.performed += performCallback; // Add perform callback to action
    if (cancelCallback != null)
    {
      action.canceled += ctx => cancelCallback(); // Add cancel callback if provided
    }

    // Store in dictionary for reference and easy unsubscription later
    _actionMap[action] = performCallback;
  }

  protected void RegisterActionComplexCancel(InputAction action, Action<InputAction.CallbackContext> performCallback, Action<InputAction.CallbackContext> cancelCallback)
  {
    action.performed += performCallback; // Add perform callback to action
    if (cancelCallback != null)
    {
      action.canceled += cancelCallback; // Add cancel callback if provided
    }

    // Store in dictionary for reference and easy unsubscription later
    _actionMap[action] = performCallback;
  }

  /// <summary>
  /// Unregisters an input action and its associated callbacks, removing it from the dictionary.
  /// This method is useful if specific actions need to be disabled or modified dynamically.
  /// </summary>
  /// <param name="action">The input action to unregister.</param>
  /// <param name="performCallback">The callback associated with the action's perform event.</param>
  /// <param name="cancelCallback">Optional callback associated with the action's cancel event.</param>
  protected void UnregisterAction(InputAction action, Action<InputAction.CallbackContext> performCallback, Action cancelCallback = null)
  {
    action.performed -= performCallback; // Remove perform callback from action
    if (cancelCallback != null)
    {
      action.canceled -= ctx => cancelCallback(); // Remove cancel callback if provided
    }
  }
}

// Networked version of the InputHandlerBase class
/*

/// <summary>
/// Abstract base class for handling input actions.
/// </summary>
public abstract class NetworkedInputHandlerBase : NetworkBehaviour
{
  protected PlayerInputActions _inputActions;

  // Dictionary to store input actions and their callbacks
  protected Dictionary<InputAction, Action<InputAction.CallbackContext>> _actionMap = new();

  public void OnEnable() {
    _inputActions = InputReader.Instance.InputActions;
    InitializeActionMap();
    // Subscribe all actions in the dictionary
    foreach (var action in _actionMap) {
      action.Key.performed += action.Value;
    }
  }

  private void OnDisable() {
    // Unsubscribe all actions in the dictionary
    foreach (var action in _actionMap) {
      action.Key.performed -= action.Value;
    }
  }

  /// <summary>
  /// Derived classes must implement this method to define which input actions to handle
  /// and map each action to its respective callback. This function is called in OnEnableFunction.
  /// </summary>
  protected abstract void InitializeActionMap();

  /// <summary>
  /// Registers an input action with its associated callback functions for performing and canceling the action.
  /// This method maps the input action to a callback, allowing easy management and unsubscription.
  /// </summary>
  /// <param name="action">The input action to register (e.g., jump, move).</param>
  /// <param name="performCallback">The callback function to invoke when the action is performed (e.g., button press).</param>
  /// <param name="cancelCallback">Optional callback for when the action is canceled (e.g., button release).</param>
  protected void RegisterAction(InputAction action, Action<InputAction.CallbackContext> performCallback, Action cancelCallback = null) {
    action.performed += performCallback; // Add perform callback to action
    if (cancelCallback != null) {
      action.canceled += ctx => cancelCallback(); // Add cancel callback if provided
    }

    // Store in dictionary for reference and easy unsubscription later
    _actionMap[action] = performCallback;
  }

  /// <summary>
  /// Unregisters an input action and its associated callbacks, removing it from the dictionary.
  /// This method is useful if specific actions need to be disabled or modified dynamically.
  /// </summary>
  /// <param name="action">The input action to unregister.</param>
  /// <param name="performCallback">The callback associated with the action's perform event.</param>
  /// <param name="cancelCallback">Optional callback associated with the action's cancel event.</param>
  protected void UnregisterAction(InputAction action, Action<InputAction.CallbackContext> performCallback, Action cancelCallback = null) {
    action.performed -= performCallback; // Remove perform callback from action
    if (cancelCallback != null) {
      action.canceled -= ctx => cancelCallback(); // Remove cancel callback if provided
    }
  }
}
*/
