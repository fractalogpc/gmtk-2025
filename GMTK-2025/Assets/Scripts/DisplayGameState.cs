using System;
using TMPro;
using UnityEngine;
public class DisplayGameState : MonoBehaviour
{
	public static DisplayGameState Instance { get; private set; }

	[SerializeField] private GameManager gameManager;
	[SerializeField] private TextMeshProUGUI countdownText;
	[SerializeField] private TextMeshProUGUI dayText;
	[SerializeField] private TextMeshProUGUI quotaText;
	[SerializeField] private ClockManager clockManager;

	public int currentSheepCount = 0;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		gameManager = GameManager.Instance;
	}

	private void Update()
	{
		if (gameManager.gameState == GameManager.GameState.CollectSheep)
		{
			clockManager.UpdateClock(gameManager.timeLeftInDay, gameManager.dayLengthMinutes * 60, false);
			countdownText.text = TimeSpan.FromSeconds(gameManager.timeLeftInDay).ToString(@"m\:ss");
			dayText.text = $"Day {gameManager.currentDay.ToString()}";
			quotaText.text = $"{currentSheepCount} / {gameManager.sheepQuota.ToString()} sheep";
		}
		else
		{
			clockManager.UpdateClock(gameManager.timeLeftInDay, gameManager.dayLengthMinutes, true);
			countdownText.text = "";
			dayText.text = $"Day {gameManager.currentDay.ToString()}";
			quotaText.text = $"{currentSheepCount} / {gameManager.sheepQuota.ToString()} sheep";
		}
	}
}
