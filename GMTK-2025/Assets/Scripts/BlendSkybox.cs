using UnityEngine;

public class BlendSkybox : MonoBehaviour
{

    [SerializeField] private Transform _distanceTarget;
    [SerializeField] private float _distance = 50f;
    [SerializeField] private float _blendDistance = 10f;
    [SerializeField] private Material _skyboxMaterial;

    private void Start()
    {
        if (_skyboxMaterial != null)
        {
            RenderSettings.skybox = _skyboxMaterial;
        }
    }

    private void Update()
    {
        if (_distanceTarget == null || _skyboxMaterial == null)
            return;

        float currentDistance = Vector3.Distance(transform.position, _distanceTarget.position);
        // Debug.Log($"Current Distance: {currentDistance}");
        float blendFactor = Mathf.Clamp01((currentDistance - _distance) / _blendDistance);

        // Set the blend factor in the shader
        _skyboxMaterial.SetFloat("_CubemapTransition", blendFactor);
        // Debug.Log($"Blend Factor: {blendFactor}");
    }

}
