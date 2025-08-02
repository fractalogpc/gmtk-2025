using UnityEngine;

public class IntermittentRender : MonoBehaviour
{
    public Camera cameraToRender;
    public float renderInterval = 1.0f; // Time in seconds between renders
    private float timeSinceLastRender;

    void Start()
    {
        if (cameraToRender == null)
        {
            Debug.LogError("Camera to render is not assigned.");
        }

        timeSinceLastRender = renderInterval; // Initialize to allow immediate render

        cameraToRender.enabled = false; // Ensure the camera is disabled
    }

    void Update()
    {
        timeSinceLastRender += Time.deltaTime;

        if (timeSinceLastRender >= renderInterval)
        {
            cameraToRender.Render();
            timeSinceLastRender = 0f; // Reset the timer
        }
    }

}
