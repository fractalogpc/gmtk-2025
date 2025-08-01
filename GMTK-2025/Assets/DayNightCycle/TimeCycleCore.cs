using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

[Serializable]
public class TimeOfDay
{
	private float _secsElapsed;
	private int _dayLength;
	private bool _zeroStart;

	// For inspector debugging
	[SerializeField] private int _day;
	[SerializeField] private int _hour;
	[SerializeField] private int _minute;
	[SerializeField] private int _second;

	/// <summary>
	/// Real seconds elapsed since the start of the game. Note that this does not automatically update, you need to call Tick() or SetTime() to update it.
	/// </summary>
	public float SecsElapsed => _secsElapsed;

	/// <summary>
	/// How long is one GameDay, in seconds.
	/// </summary>
	public int DayLength => _dayLength;

	/// <summary>
	/// If the game starts at Day 0, 00:00:00, or Day 1, 00:00:00.
	/// </summary>
	public bool ZeroStart {
		get => _zeroStart;
		set => _zeroStart = value;
	}

	/// <summary>
	/// If the TimeOfDay is currently night. Night is defined as 6 PM to 6 AM.
	/// </summary>
	public bool IsNight => GameHour < 6 || GameHour >= 18;

	/// <summary>
	/// The current day of the game. If the game starts at Day 0, this will be 0 on the first day.
	/// </summary>
	public int GameDay => Mathf.FloorToInt(_secsElapsed / _dayLength) + (_zeroStart ? 0 : 1);
	/// <summary>
	/// The current hour of the game. 0-23.
	/// </summary>
	public int GameHour => Mathf.FloorToInt(_secsElapsed / ((float)_dayLength / 24)) % 24;
	/// <summary>
	/// The current minute of the game. 0-59.
	/// </summary>
	public int GameMinute => Mathf.FloorToInt(_secsElapsed / ((float)_dayLength / 1440)) % 60;
	/// <summary>
	/// The current second of the game. 0-59.
	/// </summary>
	public int GameSecond => Mathf.FloorToInt(_secsElapsed / ((float)_dayLength / 86400)) % 60;

	public TimeOfDay(float startTime, int dayLength, bool zeroStart = false) {
		_dayLength = dayLength;
		_zeroStart = zeroStart;
		SetTime(startTime);
	}

	/// <summary>
	/// Sets the TimeOfDay to a specific time. Useful for corrections on an existing TimeOfDay object. 
	/// </summary>
	/// <param name="secondsSinceStart">Amount of seconds since Day 1, 00:00:00.</param>
	public void SetTime(float secondsSinceStart) {
		_secsElapsed = secondsSinceStart;
		UpdateDisplayVariables();
	}

	/// <summary>
	/// Sets the DayLength of the TimeOfDay. Beware of time inconsistencies when changing this value.
	/// </summary>
	/// <param name="dayLength">Length of day, in seconds.</param>
	public void SetDayLength(int dayLength) {
		_dayLength = dayLength;
	}

	/// <summary>
	/// Tick the TimeOfDay forward by a certain amount of real seconds. Make sure to take into account the current DayLength.
	/// </summary>
	/// <param name="seconds">Seconds to tick forward by.</param>
	public void Tick(float seconds) {
		_secsElapsed += seconds;
		UpdateDisplayVariables();
	}

	private void UpdateDisplayVariables() {
		_day = GameDay;
		_hour = GameHour;
		_minute = GameMinute;
		_second = GameSecond;
	}
}

// Mirror has its own NetworkTime.time for syncing time across the network,
// But we want to have a synced time of day for our day night cycle and other things
// [RequireComponent(typeof(NetworkIdentity))]
// public class TimeCycleCore : NetworkBehaviour
public class TimeCycleCore : MonoBehaviour
{
	private double _networkTime;
	[Tooltip("The hour that the game starts at. 6:00 is sunrise, and 18:00 is sunset. Follows 24-hour format.")]
	[SerializeField][Range(0, 24)] private float _startGameHour;
	[Tooltip("How long one in-game day lasts, in real-time minutes.")]
	[SerializeField] private float _dayCycleInMinutes = 0.5f;
	[Tooltip("If the game starts at Day 0, 00:00:00, or Day 1, 00:00:00.")] [SerializeField]
	private bool _daysStartAtZero = false;
	
	public TimeOfDay TimeOfDay;

	// Yet to be implemented
	public UnityEvent OnDayChanged;
	public UnityEvent OnHourChanged;
	public UnityEvent OnNight;
	public UnityEvent OnDay;

	public static TimeCycleCore Instance;

	private float _lastHour;
	private float _timeOffset = 0f; // Time offset for additively loaded scene transitions

	private void Awake() {
		Singleton();
	}

	public void SetTimeOffset(float offset) {
		_timeOffset = offset;
	}

	private void Start() {
		_timeOffset = Time.time;
		// Multiplayer: Change to network time for multiplayer
		_networkTime = Time.time - _timeOffset;
		TimeOfDay = new TimeOfDay(_startGameHour * ((_dayCycleInMinutes * 60) / 24), Mathf.FloorToInt(_dayCycleInMinutes * 60), _daysStartAtZero);
	}

	private void Update() {
		_networkTime = Time.time + _startGameHour * ((_dayCycleInMinutes * 60) / 24) - _timeOffset;
		TimeOfDay.SetTime((float)_networkTime);
		
		if (TimeOfDay.GameHour != _lastHour) {
			if (TimeOfDay.IsNight && _lastHour == 17) {
				OnNight.Invoke();
			}
			else {
				OnDay.Invoke();
			}

			if (TimeOfDay.GameHour == 0) {
				OnDayChanged.Invoke();
			}
			
			_lastHour = TimeOfDay.GameHour;
			OnHourChanged.Invoke();
		}
	}

	private void Singleton() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}
}