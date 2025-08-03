using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using MoreMountains.Feedbacks;
using FMODUnity;

public class EndingCutscene : MonoBehaviour
{

    [SerializeField] private GameObject cutsceneCamera;
    [SerializeField] private AnimationCurve rocketLaunchCurve;
    [SerializeField] private float rocketLaunchDuration = 5f;
    [SerializeField] private Transform rocketTransform;
    [SerializeField] private float rocketLaunchHeight = 10f;
    [SerializeField] private ParticleSystem[] rocketThrusters;
    [SerializeField] private Outline doorOutline;
    [SerializeField] private Animation[] doorAnimations;
    [SerializeField] private GameObject player;
    [SerializeField] private FadeElementInOut fadeToBlack;
    [SerializeField] private GameObject[] canvasesToDisable;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private Transform[] points;
    [SerializeField] private StudioEventEmitter rocketSound;
    [SerializeField] private StudioEventEmitter winMusic;
    [SerializeField] private GameObject creditsScreen;

    private bool canTriggerCutscene = false;

    private Vector3 initialRocketPosition;

    public void EnableCutsceneTrigger()
    {
        canTriggerCutscene = true;
    }

    public void TriggerCutscene()
    {
        if (canTriggerCutscene)
        {
            StartCoroutine(PlayCutscene());
        }
    }

    private void Start()
    {
        initialRocketPosition = rocketTransform.position;
    }

    private IEnumerator PlayCutscene()
    {
        // Fade to black
        fadeToBlack.FadeIn();
        yield return new WaitForSeconds(1f);

        cutsceneCamera.SetActive(true);
        player.SetActive(false); // Hide player during cutscene
        cameraShake.SetIntensity(0f);
        foreach (var canvas in canvasesToDisable)
        {
            canvas.SetActive(false); // Disable any UI elements that should not be visible during the cutscene
        }
        doorOutline.enabled = false;
        yield return new WaitForSeconds(0.5f);
        winMusic.Play();
        fadeToBlack.FadeOut();
        yield return new WaitForSeconds(1f);

        // Start music

        // Open door
        foreach (var animation in doorAnimations)
        {
            animation.Play();
        }

        SheepReception.Instance.ReleaseAllSheep(System.Array.ConvertAll(points, p => p.position));

        yield return new WaitForSeconds(doorAnimations[0].clip.length);
        // Make sheep go into the rocket

        // Wait for sheep to reach the rocket
        yield return new WaitForSeconds(15f);

        SheepReception.Instance.FreezeAllSheep();

        // Start engines
        foreach (var thruster in rocketThrusters)
        {
            thruster.Play();
        }

        rocketSound.Play();

        // Launch rocket
        float elapsed = 0f;
        bool faded = false;
        while (elapsed < rocketLaunchDuration)
        {
            float t = elapsed / rocketLaunchDuration;
            float height = rocketLaunchCurve.Evaluate(t) * rocketLaunchHeight;
            rocketTransform.position = initialRocketPosition + new Vector3(0, height, 0);
            cameraShake.SetIntensity(1f - rocketLaunchCurve.Evaluate(t));
            elapsed += Time.deltaTime;
            if (t >= 0.8f && !faded)
            {
                faded = true;
                fadeToBlack.FadeIn();
            }
            yield return null;
        }

        // Bring up credits screen
        creditsScreen.SetActive(true);

        yield return new WaitForSeconds(28f);

        winMusic.Stop();

        // Exit to menu
    }
    
}
