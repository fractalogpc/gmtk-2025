using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Flicker : MonoBehaviour
{
    [SerializeField] private Light flickerLight;
    [SerializeField] private Renderer flickerRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Vector2 flickerRange = new Vector2(0.5f, 1.5f);
    [SerializeField] private Vector2 flickerDelayRange = new Vector2(0.1f, 0.5f);

    private float originalIntensity;

    private void Start()
    {
        originalIntensity = flickerLight.intensity;
        StartCoroutine(FlickerLight());
    }

    private IEnumerator FlickerLight()
    {
        while (true)
        {
            float flickerTime = Random.Range(flickerRange.x, flickerRange.y);
            flickerLight.intensity = 0;
            flickerRenderer.material = offMaterial;
            yield return new WaitForSeconds(flickerTime);
            flickerLight.intensity = originalIntensity;
            flickerRenderer.material = onMaterial;
            // Random delay before the next flicker
            float delay = Random.Range(flickerDelayRange.x, flickerDelayRange.y);
            yield return new WaitForSeconds(delay);
        }
    }
}
