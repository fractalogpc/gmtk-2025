using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseHandler : InputHandlerBase
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button quitButton;
    [SerializeField] private FadeElementInOut fadeToBlack;
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

    private void Start()
    {
	    base.Start();
        quitButton.onClick.AddListener(ButtonQuitToMenu);
	    pauseMenu.SetActive(false);
    }

    private void OnDestroy()
    {
	    quitButton.onClick.RemoveAllListeners();
	    _inputActions.Player.Disable();
	    _inputActions.GenericUI.Disable();
    }

    private void ButtonQuitToMenu()
    {
	    Debug.Log("Quitting to menu...");
	    fadeToBlack.FadeIn();
        gameManager.StopGame();
	    _inputActions.Player.Disable();
	    StartCoroutine(GoToMenuCoroutine());
    }

    private IEnumerator GoToMenuCoroutine()
    {
	    Time.timeScale = 1f; // Reset time scale before loading the menu
	    yield return new WaitForSeconds(fadeToBlack.FadeInTime);
	    SceneManager.LoadScene("MainMenu");
    }
}
