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

	private void Start() {
		cooldown = Single.NegativeInfinity;
		controllerRunningLastFrame = sheepController.running;
	}

	private bool JustStartedRunning() {
		bool running = sheepController.running;
		if (!running) return false;
		if (controllerRunningLastFrame) return false;
		return true;
	}

	private void Update() {
		if (Vector3.Distance(transform.position, Camera.main.gameObject.transform.position) >= muteDistance) return;
		if (JustStartedRunning()) {
			cooldown = 0;
		}
		cooldown -= Time.deltaTime;
		if (cooldown <= 0) {
			if (sheepController.running) {
				print("sheep playing idle");
				afraidSoundEmitter.Play();
			}
			else {
				idleSoundEmitter.Play();
			}
			cooldown = Random.Range(soundCooldownMin, soundCooldownMax);
		}
		controllerRunningLastFrame = sheepController.running;
	}
}
