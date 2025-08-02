using System;
using TMPro;
using UnityEngine;
public class DisplayGameState : MonoBehaviour
{
	[SerializeField] private GameManager gameManager;
	[SerializeField] private TextMeshProUGUI countdownText;
	[SerializeField] private TextMeshProUGUI dayText;
	[SerializeField] private TextMeshProUGUI quotaText;
	[SerializeField] private ClockManager clockManager;

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
			quotaText.text = $"{gameManager.sheepQuota.ToString()} sheep";
		}
		else
		{
			clockManager.UpdateClock(gameManager.timeLeftInDay, gameManager.dayLengthMinutes, true);
			countdownText.text = "";
			dayText.text = $"Day {gameManager.currentDay.ToString()}";
			quotaText.text = $"{gameManager.numSheepOffered.ToString()} / {gameManager.sheepQuota.ToString()} sheep";
		}
	}
}
