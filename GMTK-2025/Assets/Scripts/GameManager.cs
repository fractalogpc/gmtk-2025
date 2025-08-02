using System;
using System.Collections;
using Player;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        CollectSheep,
        OfferSheep,
    }

    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private PitManager pitManager;
    [SerializeField] private Transform playerStart;
    public int sheepQuota {
        get;
        private set;
    }
    public int currentDay {
        get;
        private set;
    }

    public float timeLeftInDay {
        get;
        private set;
    }
    [SerializeField] private FadeElementInOut fadeToBlack;
    [Header("Settings")]
    [SerializeField] private float dayLengthMinutes;
    [SerializeField] private int startQuota;
    [SerializeField] private int dailyQuotaIncrease;

    public GameState gameState;
    
    private int numSheepOffered = 0;
    private bool hasOfferedThisDay = false;
    
    public float startDelay = 2f;
    bool initialized = false;
    private float initalizationTimer = 0f;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerObject = GameObject.Find("Player");
        playerStart = GameObject.FindWithTag("PlayerStart").transform;
        pitManager = GameObject.FindWithTag("PitManager").GetComponent<PitManager>();
        Initialize();
        StartCoroutine(GameLogic());
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

    private IEnumerator GameLogic() {
        sheepQuota = startQuota;
        currentDay = 1;
        while (true) {
            gameState = GameState.CollectSheep;
            ResetPlayerToStart();
            StartDay(currentDay);
            yield return new WaitForSeconds(SetPlayerVision(true));
            while (timeLeftInDay > 0) {
                timeLeftInDay -= Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(SetPlayerVision(false));
            ResetPlayerToStart();
            gameState = GameState.OfferSheep;
            yield return new WaitForSeconds(SetPlayerVision(true));
            pitManager.SetOfferable(true);
            while (!hasOfferedThisDay) {
                yield return null;
            }
            yield return new WaitForSeconds(5f);
            if (numSheepOffered < sheepQuota) {
                Lose();
                break;
            }
            yield return new WaitForSeconds(SetPlayerVision(false));
            currentDay++;
        }
    }

    private void StartDay(int numDays) {
        timeLeftInDay = dayLengthMinutes * 60;
        sheepQuota = numDays * dailyQuotaIncrease;
        numSheepOffered = 0;
        hasOfferedThisDay = false;
        gameState = GameState.CollectSheep;
        pitManager.SetOfferable(false);
    }

    private void ResetPlayerToStart() {
        playerObject.GetComponent<PlayerController>().SetPosition(playerStart.position);
        playerObject.GetComponent<PlayerController>().SetRotation(playerStart.rotation);
    }

    private float SetPlayerVision(bool setTrue) {
        if (setTrue) {
            fadeToBlack.FadeOut();
            return fadeToBlack.FadeInTime;
        }
        fadeToBlack.FadeIn();
        return fadeToBlack.FadeOutTime;
    }

    public void AddToQuota(int amount) {
        numSheepOffered += amount;
        hasOfferedThisDay = true;
    }

    private void Lose() {
        
    }
}