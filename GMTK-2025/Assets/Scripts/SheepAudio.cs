using System;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

public class SheepAudio : MonoBehaviour
{
	[SerializeField] private AdvancedSheepController sheepController;
	[SerializeField] private StudioEventEmitter idleSoundEmitter;
	[SerializeField] private StudioEventEmitter afraidSoundEmitter;
	[SerializeField] private float soundCooldownMin, soundCooldownMax;
	[SerializeField] private float muteDistance = 20f;

	private bool controllerRunningLastFrame = false;
	private float cooldown;

	private Camera mainCamera;

	private void Start()
	{
		cooldown = Single.NegativeInfinity;
		controllerRunningLastFrame = sheepController.running;
		mainCamera = Camera.main;
	}

	private bool JustStartedRunning()
	{
		bool running = sheepController.running;
		if (!running) return false;
		if (controllerRunningLastFrame) return false;
		return true;
	}

	public void UpdateSound()
	{
		if ((transform.position - mainCamera.transform.position).sqrMagnitude >= muteDistance * muteDistance) return;

		bool isRunning = sheepController.running;

		if (isRunning && !controllerRunningLastFrame)
		{
			cooldown = 0;
		}

		cooldown -= Time.deltaTime;

		if (cooldown <= 0f)
		{
			if (isRunning)
			{
#if UNITY_EDITOR
				Debug.Log("sheep playing idle");
#endif
				if (!afraidSoundEmitter.IsPlaying())
				{
					afraidSoundEmitter.Play();
				}
			}
			else
			{
				if (!idleSoundEmitter.IsPlaying())
				{
					idleSoundEmitter.Play();
				}
			}
			cooldown = Random.Range(soundCooldownMin, soundCooldownMax);
		}

		controllerRunningLastFrame = isRunning;
	}

}
