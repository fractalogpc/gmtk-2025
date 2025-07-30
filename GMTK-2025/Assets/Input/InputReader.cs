using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    public PlayerInputActions InputActions { get; private set; }

    public static event Action<InputMap> OnBeforeInputMapChange;
    public static event Action<InputMap> OnAfterInputMapChange;

    private const InputMap DEFAULT_MAP = InputMap.Player;
    public InputMap CurrentMap { get; private set; } = InputMap.Null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InputActions = new PlayerInputActions();
        InputActions.Enable();
    }

    public void SwitchInputMap(InputMap newInputMap)
    {
        if (newInputMap == CurrentMap)
        {
            Debug.LogWarning($"Trying to change to the same input map: {newInputMap}");
            return;
        }

        OnBeforeInputMapChange?.Invoke(newInputMap);

        CurrentMap = newInputMap;
        Debug.Log($"<color=grey>Changing Input Map:</color> {newInputMap}");
        switch (newInputMap)
        {
            case InputMap.GenericUI:
                InputActions.Player.Disable();
                InputActions.GenericUI.Enable();
                break;
            case InputMap.Player:
                InputActions.GenericUI.Disable();
                InputActions.Player.Enable();
                break;
            case InputMap.Null:
                InputActions.GenericUI.Disable();
                InputActions.Player.Disable();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newInputMap), newInputMap, null);
        }

        OnAfterInputMapChange?.Invoke(newInputMap);

        // Debug.Log($"<color=grey>Changed Input Map:</color> {newInputMap}");
    }
}

[Serializable]
public enum InputMap
{
  Null = -1,
  GenericUI = 0,
  Player = 1,
}