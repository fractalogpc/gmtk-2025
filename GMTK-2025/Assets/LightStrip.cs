using UnityEngine;
using System.Collections;

public class LightStrip : MonoBehaviour
{

    [SerializeField] private Renderer[] bulbRenderers;
    [SerializeField] private Color LightColor = Color.white;
    [SerializeField] private float lightIntensity = 1f;
    [SerializeField] private float timePerBulb = 0.1f;

    void Start()
    {
        // Give each bulb a material instance to avoid shared material issues
        foreach (var renderer in bulbRenderers)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.EnableKeyword("_EMISSION");
        }

        StartCoroutine(AnimateLightStrip());
    }

    private IEnumerator AnimateLightStrip()
    {
        while (true)
        {
            for (int i = 0; i < bulbRenderers.Length; i++)
            {
                bulbRenderers[i].material.SetColor("_EmissionColor", LightColor * lightIntensity);
                yield return new WaitForSeconds(timePerBulb);
                bulbRenderers[i].material.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
