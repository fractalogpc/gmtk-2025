using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Serialization;

public class LightningVFXManager : MonoBehaviour
{
	[FormerlySerializedAs("playerTransform")][SerializeField] private Transform _playerTransform;
	[SerializeField] private Vector2 _lightningDelayRange = new(0.5f, 1.5f);
	[SerializeField] private Vector2 lightningRadius;
	[SerializeField] private LayerMask lightningStrikeLayerMask;

	private VisualEffect lightningVFX;
	private VFXEventAttribute lightningPositionAttribute;

	private float timeUntilNextLightning;

	public void SetDelayRangeMin(float min) {
		_lightningDelayRange.x = min;
	}
	public void SetDelayRangeMax(float max) {
		_lightningDelayRange.y = max;
	}

	private void Awake() {
		timeUntilNextLightning = Random.Range(_lightningDelayRange.x, _lightningDelayRange.y);
	}

	private void Start() {
		lightningVFX = GetComponent<VisualEffect>();
		if (lightningVFX == null) {
			Debug.LogError("Lightning VFX component not found on this GameObject.");
			return;
		}

		lightningPositionAttribute = lightningVFX.CreateVFXEventAttribute();
	}

	private void Update() {
		timeUntilNextLightning -= Time.deltaTime;

		if (timeUntilNextLightning <= 0f) {
			// Randomize the delay for the next lightning strike
			timeUntilNextLightning = Random.Range(_lightningDelayRange.x, _lightningDelayRange.y);

			// Randomize the position of the lightning strike within a radius around the player
			float radius = Random.Range(lightningRadius.x, lightningRadius.y);
			Vector3 randomDirection = Random.insideUnitSphere.normalized * radius;
			randomDirection.y = 0; // Keep it horizontal
			Vector3 lightningPosition = _playerTransform.position + randomDirection;

			RaycastHit hit;
			if (Physics.Raycast(lightningPosition + Vector3.up * 100f, Vector3.down, out hit, 5000f, lightningStrikeLayerMask))
				SpawnLightning(hit.point);
			else
				Debug.LogWarning("No valid surface found for lightning strike.");
		}
	}

	private void SpawnLightning(Vector3 position) {
		// Set the position of the lightning effect to the player's position
		lightningPositionAttribute.SetVector3("targetPosition", position);
		lightningVFX.SendEvent("SpawnLightning", lightningPositionAttribute);
	}
}