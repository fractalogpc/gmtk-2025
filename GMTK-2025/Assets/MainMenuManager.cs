using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private FadeElementInOut fadeToBlack;
    public StudioEventEmitter normalMusicEmitter;
    public string gameSceneName = "RealEnvironment";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);

        // // Ensure buttons are interactable
        // startButton.interactable = true;
        // quitButton.interactable = true;
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        startButton.onClick.RemoveListener(StartGame);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    private void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        fadeToBlack.FadeIn();
        normalMusicEmitter.Stop();
        // startButton.interactable = false;
        // quitButton.interactable = false;
        yield return new WaitForSeconds(fadeToBlack.FadeInTime);
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        
        // If running in the editor, stop playing the scene
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
