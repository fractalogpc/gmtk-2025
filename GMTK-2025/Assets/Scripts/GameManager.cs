using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform playerStart;
    [SerializeField] private int sheepQuota;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float timeLeftInDay;
    [Header("Settings")]
    [SerializeField] private float sheepCollectionTime;
    [SerializeField] private int dailyQuotaIncrease;

    private int numSheepOffered = 0;
    
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
        Initialize();
        GameLogic();
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
        while (true) {
            StartDay(currentDay);
            while (timeLeftInDay > 0) {
                timeLeftInDay -= Time.deltaTime;
                yield return null;
            }
            if (numSheepOffered < sheepQuota) {
                Lose();
                break;
            }
            currentDay++;
        }
    }

    private void StartDay(int numDays) {
       playerObject.transform.position = playerStart.position;
       timeLeftInDay = sheepCollectionTime;
       sheepQuota = numDays * dailyQuotaIncrease;
       numSheepOffered = 0;
    }

    public void AddToQuota(int amount) {
        numSheepOffered += amount;
    }

    private void Lose() {
        
    }
}