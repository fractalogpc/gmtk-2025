using System;
using TMPro;
using UnityEngine;
public class DisplayGameState : MonoBehaviour
{
	[SerializeField] private GameManager gameManager;
	[SerializeField] private TextMeshProUGUI countdownText;
	[SerializeField] private TextMeshProUGUI dayText;
	[SerializeField] private TextMeshProUGUI quotaText;

	private void Start() {
		gameManager = GameManager.Instance;
	}

	private void Update() {
		countdownText.text = TimeSpan.FromSeconds(gameManager.timeLeftInDay).ToString(@"mm\:ss");
		dayText.text = $"Day {gameManager.currentDay.ToString()}";
		quotaText.text = $"Need {gameManager.sheepQuota.ToString()} sheep";
	}
}
