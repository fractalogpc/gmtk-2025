using UnityEngine;

public class GameManager : MonoBehaviour
{

    public float startDelay = 2f;

    private float timer = 0f;
    bool initialized = false;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Eventually add a pause
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        initialized = true;
    }

    void FixedUpdate()
    {
        if (initialized) return;
        timer += Time.fixedDeltaTime;
        if (timer >= startDelay)
        {
            Initialize();
        }
    }
}
