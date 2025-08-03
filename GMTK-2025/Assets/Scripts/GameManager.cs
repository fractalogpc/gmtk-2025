using System;
using System.Collections;
using FMODUnity;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : InputHandlerBase
{
  public static GameManager Instance;

  public enum GameState
  {
    CollectSheep,
    OfferSheep,
  }

  [Header("References")]
  public GameObject playerObject;
  [SerializeField] private PitManager pitManager;
  [SerializeField] private Transform playerStart;
  [SerializeField] private StudioEventEmitter normalMusicEmitter;
  [SerializeField] private StudioEventEmitter ambientSoundEmitter;
  [SerializeField] private StudioEventEmitter scaryMusicEmitter;
  [SerializeField] private StudioEventEmitter lostMusicEmitter;

  [SerializeField] private int[] dailyQuotas;
  [SerializeField] private int dailyQuotaIncreasesAfterDay5;

  public int sheepQuota
  {
    get;
    private set;
  }
  public int currentDay
  {
    get;
    private set;
  }

  public float timeLeftInDay
  {
    get;
    private set;
  }
  [SerializeField] private FadeElementInOut fadeToBlack;
  [SerializeField] private FadeElementInOut topLevelFadeToBlack;
  [SerializeField] private GameObject loseScreen;
  [SerializeField] private FadeElementInOut loseScreenFade;
  [Header("Settings")]
  public float dayLengthMinutes;
  [SerializeField] private int startQuota;
  [SerializeField] private Light sunLight;
  [SerializeField] private Material skyboxMaterial;
  [SerializeField] private Color dayFogColor;
  [SerializeField] private Color nightFogColor;
  [SerializeField] private GameObject lightning;

  public UnityEvent onDayStart;
  public UnityEvent onNightStart;

  public GameState gameState;

  public int numSheepOffered = 0;
  private bool hasOfferedThisDay = false;

  public float startDelay = 2f;
  bool initialized = false;
  private float initalizationTimer = 0f;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      // If an instance already exists, destroy the old one
      Debug.LogWarning("Multiple GameManager instances found. Destroying the old one.");
      Destroy(Instance.gameObject);
      Instance = this;
    }
  }

  public override void Start()
  {
    base.Start();
    playerObject = GameObject.Find("Player");
    playerStart = GameObject.FindWithTag("PlayerStart").transform;
    pitManager = GameObject.FindWithTag("PitManager").GetComponent<PitManager>();
    Initialize();
    StartCoroutine(GameLogic());
    fadeToBlack.Show();
    topLevelFadeToBlack.Hide();
    _inputActions.Player.Enable();
    _inputActions.GenericUI.Disable();
  }

  private void Initialize()
  {
    // Eventually add a pause
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    Time.timeScale = 1f;

    initialized = true;
  }

  void FixedUpdate()
  {
    if (initialized) return;
    initalizationTimer += Time.fixedDeltaTime;
    if (initalizationTimer >= startDelay)
    {
      Initialize();
    }
  }

  private IEnumerator GameLogic()
  {
    sheepQuota = startQuota;
    currentDay = 1;
    while (true)
    {

      // Regenerate current sheep's wool
      SheepSpawner.Instance.RegenerateAllPenSheep();

      // Spawn sheep
      SheepSpawner.Instance.GenerateSheep();

      gameState = GameState.CollectSheep;
      ResetPlayerToStart();
      StartDay(currentDay);

      onDayStart?.Invoke();

      // Set skybox to day
      skyboxMaterial.SetFloat("_CubemapTransition", 1f);
      RenderSettings.fogColor = dayFogColor;
      sunLight.enabled = true;
      lightning.SetActive(false);

      normalMusicEmitter.Play();
      yield return new WaitForSeconds(SetPlayerVision(true));
      // Disabled after offer, need to re-enable
      if (!_inputActions.Player.Move.enabled)
      {
        _inputActions.Player.Move.Enable();
      }
      while (timeLeftInDay > 0)
      {
        if (isWinCutsceneActive)
        {
          yield return null;
          continue;
        }
        timeLeftInDay -= Time.deltaTime;
        ambientSoundEmitter.SetParameter("Nighttime", 1 - (timeLeftInDay / (dayLengthMinutes * 60)));
        ambientSoundEmitter.SetParameter("Creepiness", Mathf.Clamp01(Vector3.Distance(playerObject.transform.position, pitManager.transform.position) / 300f));
        yield return null;
      }
      ambientSoundEmitter.SetParameter("Nighttime", 1f);
      ambientSoundEmitter.SetParameter("Creepiness", 1f);
      normalMusicEmitter.Stop();
      yield return new WaitForSeconds(SetPlayerVision(false));
      ResetPlayerToStart();
      gameState = GameState.OfferSheep;
      onNightStart?.Invoke();

      // Environmental changes for night
      skyboxMaterial.SetFloat("_CubemapTransition", 0f);
      RenderSettings.fogColor = nightFogColor;
      sunLight.enabled = false;
      lightning.SetActive(true);

      // Trigger nighttime objective
      ObjectiveSystem.Instance.CompleteObjectiveByName("Nighttime");

      // Remove wild sheep
      SheepSpawner.Instance.ClearWildSheep();

      yield return new WaitForSeconds(SetPlayerVision(true));
      scaryMusicEmitter.Play();
      pitManager.SetOfferable(true);
      while (!hasOfferedThisDay)
      {
        yield return null;
      }
      if (numSheepOffered < sheepQuota)
      {
        print("num sheep offered: " + numSheepOffered + ", quota: " + sheepQuota);
        Lose();
        break;
      }
      scaryMusicEmitter.Stop();
      yield return new WaitForSeconds(SetPlayerVision(false));
      currentDay++;
    }
  }

  private void StartDay(int numDays)
  {
    timeLeftInDay = dayLengthMinutes * 60;
    sheepQuota = (numDays <= 5) ? dailyQuotas[numDays - 1] : dailyQuotas[4] + (numDays - 5) * dailyQuotaIncreasesAfterDay5;
    numSheepOffered = 0;
    hasOfferedThisDay = false;
    gameState = GameState.CollectSheep;
    pitManager.SetOfferable(false);
  }

  public void ResetGame()
  {
    SceneManager.LoadScene("RealEnvironment");
  }

  private void ResetPlayerToStart()
  {
    playerObject.GetComponent<PlayerController>().SetPositionAndRotation(playerStart);
  }

  public float SetPlayerVision(bool setTrue)
  {
    if (setTrue)
    {
      fadeToBlack.FadeOut();
      return fadeToBlack.FadeOutTime;
    }
    fadeToBlack.FadeIn();
    return fadeToBlack.FadeInTime;
  }

  public void AddToQuota(int amount)
  {
    numSheepOffered += amount;
    hasOfferedThisDay = true;
  }

  public void StopGameMusicAndAmbient()
  {
    normalMusicEmitter.Stop();
    ambientSoundEmitter.Stop();
    scaryMusicEmitter.Stop();
    lostMusicEmitter.Stop();
    // scaryMusicEmitter.Stop();
  }
  
  public void ExitToMenu()
  {
    StartCoroutine(ExitToMenuCoroutine());
  }

  private IEnumerator ExitToMenuCoroutine()
  {
	  Debug.Log("Quitting to menu...");
    Time.timeScale = 1f; // Reset time scale so WaitForSeconds works correctly
    _inputActions.Player.Disable();
    _inputActions.GenericUI.Disable();
    topLevelFadeToBlack.FadeIn();
    StopGameMusicAndAmbient();
    yield return new WaitForSeconds(topLevelFadeToBlack.FadeInTime);
    SceneManager.LoadScene("MainMenu");
  }

  private void Lose()
  {
    Debug.Log("You lost! Not enough sheep offered.");
    pitManager.EatPlayer();
    StopGameMusicAndAmbient();
    StartCoroutine(LoseCoroutine());
    SetPlayerInput(false);
  }

  private IEnumerator LoseCoroutine()
  {
    yield return new WaitForSeconds(1f);
    SetPlayerVision(false);
    yield return new WaitForSeconds(1f);
    lostMusicEmitter.Play();
    yield return new WaitForSeconds(1f);
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
    loseScreenFade.Hide();
    loseScreen.SetActive(true);
    loseScreenFade.FadeIn(true);
  }
  
  private bool isWinCutsceneActive = false;
  public void StopForWinCutscene()
  {
    isWinCutsceneActive = true;
    SetPlayerMovement(false);
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }
  
  public void SetPlayerInput(bool enabled)
  {
    if (enabled)
    {
      _inputActions.Player.Enable();
    }
    else
    {
      _inputActions.Player.Disable();
    }
  }
  
  public void SetPlayerMovement(bool enabled)
  {
    if (enabled)
    {
      _inputActions.Player.Move.Enable();
    }
    else
    {
      _inputActions.Player.Move.Disable();
    }
  }

  protected override void InitializeActionMap()
  {
  }
}