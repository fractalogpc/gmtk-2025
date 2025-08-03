using System;
using System.Collections;
using FMODUnity;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

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
  [Header("Settings")]
  public float dayLengthMinutes;
  [SerializeField] private int startQuota;
  [SerializeField] private int dailyQuotaIncrease;

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
      Destroy(gameObject);
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
      gameState = GameState.CollectSheep;
      ResetPlayerToStart();
      StartDay(currentDay);
      normalMusicEmitter.Play();
      yield return new WaitForSeconds(SetPlayerVision(true));
      // Disabled after offer, need to re-enable
      if (!_inputActions.Player.Move.enabled)
      {
        _inputActions.Player.Move.Enable();
      }
      while (timeLeftInDay > 0)
      {
        timeLeftInDay -= Time.deltaTime;
        ambientSoundEmitter.SetParameter("Nighttime", timeLeftInDay / (dayLengthMinutes * 60));
        ambientSoundEmitter.SetParameter("Creepiness", Mathf.Clamp01(Vector3.Distance(playerObject.transform.position, pitManager.transform.position) / 300f));
        yield return null;
      }
      normalMusicEmitter.Stop();
      yield return new WaitForSeconds(SetPlayerVision(false));
      ResetPlayerToStart();
      gameState = GameState.OfferSheep;
      yield return new WaitForSeconds(SetPlayerVision(true));
      pitManager.SetOfferable(true);
      while (!hasOfferedThisDay)
      {
        yield return null;
      }
      if (numSheepOffered < sheepQuota)
      {
        Lose();
        break;
      }
      yield return new WaitForSeconds(SetPlayerVision(false));
      currentDay++;
    }
  }

  private void StartDay(int numDays)
  {
    timeLeftInDay = dayLengthMinutes * 60;
    sheepQuota = numDays * dailyQuotaIncrease;
    numSheepOffered = 0;
    hasOfferedThisDay = false;
    gameState = GameState.CollectSheep;
    pitManager.SetOfferable(false);
  }

  private void ResetPlayerToStart()
  {
    playerObject.GetComponent<PlayerController>().SetPosition(playerStart.position);
    playerObject.GetComponent<PlayerController>().SetRotation(playerStart.rotation);
  }

  public float SetPlayerVision(bool setTrue)
  {
    if (setTrue)
    {
      fadeToBlack.FadeOut();
      return fadeToBlack.FadeInTime;
    }
    fadeToBlack.FadeIn();
    return fadeToBlack.FadeOutTime;
  }

  public void AddToQuota(int amount)
  {
    print("sheep offered");
    numSheepOffered += amount;
    hasOfferedThisDay = true;
  }

  public void StopGameMusicAndAmbient()
  {
    normalMusicEmitter.Stop();
    ambientSoundEmitter.Stop();
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
  }

  private IEnumerator LoseCoroutine()
  {
    yield return new WaitForSeconds(3f);
    SetPlayerVision(false);
    yield return new WaitForSeconds(1f);
    lostMusicEmitter.Play();
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