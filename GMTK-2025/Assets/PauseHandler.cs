using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseHandler : InputHandlerBase
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button quitButton;
    public GameManager gameManager;

    protected override void InitializeActionMap()
    {
        RegisterAction(_inputActions.Player.Pause, ctx => HandlePause(true));
        RegisterAction(_inputActions.GenericUI.CloseUI, ctx => HandlePause(false));
    }

    private void HandlePause(bool active)
    {
        if (active)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _inputActions.Player.Disable();
            _inputActions.GenericUI.Enable();
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _inputActions.Player.Enable();
            _inputActions.GenericUI.Disable();
        }
    }

    public override void Start()
    {
        base.Start();
        quitButton.onClick.AddListener(ButtonQuitToMenu);
	    pauseMenu.SetActive(false);
    }

    private void OnDestroy()
    {
	    quitButton.onClick.RemoveAllListeners();
    }

    private void ButtonQuitToMenu()
    {
        gameManager.ExitToMenu();
    }
}