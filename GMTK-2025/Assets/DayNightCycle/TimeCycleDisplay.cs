using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimeCycleDisplay : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _dayText;
	[SerializeField] private TextMeshProUGUI _clockText;
	
	private TimeCycleCore _timeCycleCore;
	private bool _initialized = false;
	
	private IEnumerator Start() {
		_dayText.text = "";
		_clockText.text = "";
		while (_timeCycleCore == null) {
			_timeCycleCore = FindFirstObjectByType<TimeCycleCore>();
			yield return null;
		}
		_initialized = true;
	}
	
	private void Update() {
		if (!_initialized) return;
		_dayText.text = $"Day {_timeCycleCore.TimeOfDay.GameDay}";
		_clockText.text = $"{_timeCycleCore.TimeOfDay.GameHour:00}:{_timeCycleCore.TimeOfDay.GameMinute:00}";
	}
}
