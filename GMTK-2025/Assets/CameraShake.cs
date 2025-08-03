using UnityEngine;

public class CameraShake : MonoBehaviour
{

    [SerializeField] private float shakeFrequency = 2f;
    [SerializeField] private float shakeAmplitude = 0.1f;

    private float multiplier = 1f;
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    public void SetIntensity(float intensity)
    {
        multiplier = intensity;
    }

    private void Update()
    {
        Vector3 randomOffset = Random.insideUnitSphere * shakeAmplitude * multiplier;
        randomOffset.z = 0;
        Vector3 targetPosition = initialPosition + randomOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * shakeFrequency);
    }
}
