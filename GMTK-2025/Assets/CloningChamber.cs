using UnityEngine;
using System.Collections;

public class CloningChamber : MonoBehaviour
{

    [SerializeField] private GameObject sheepPrefab;

    [SerializeField] private AnimationCurve sizeCurve;
    [SerializeField] private float cloneDuration = 5f;
    [SerializeField] private float cloneDelay = 2f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform spinPlate;
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private Animation doorAnimation;

    [SerializeField] private Vector3 initialSheepMovePoint = new Vector3(0, 0, 0f);

    private bool isCloning = false;

    void Start()
    {
        StartCoroutine(CloneSheep());
    }

    private IEnumerator CloneSheep()
    {
        if (isCloning)
        {
            yield break; // Prevent multiple clones from being created simultaneously
        }
        isCloning = true;
        GameObject clone = Instantiate(sheepPrefab, spawnPoint.position, Quaternion.identity);
        clone.transform.name = "ClonedSheep";
        clone.transform.SetParent(GameObject.FindWithTag("SheepSpawner").transform);

        AdvancedSheepController cloneController = clone.GetComponent<AdvancedSheepController>();
        cloneController.PlayerTransform = GameObject.FindWithTag("Player").transform;
        cloneController.woolSize = 1;
        cloneController.woolColorIndex = 0; // Default color index

        clone.transform.localScale = Vector3.zero; // Start with zero scale
        float elapsed = 0f;

        while (elapsed < cloneDuration)
        {
            float scale = sizeCurve.Evaluate(elapsed / cloneDuration);
            clone.transform.localScale = new Vector3(scale, scale, scale);
            spinPlate.localRotation *= Quaternion.Euler(0, spinSpeed * Time.deltaTime, 0); // Spin the plate
            clone.transform.localRotation *= Quaternion.Euler(0, spinSpeed * Time.deltaTime, 0); // Rotate the clone with the plate
            elapsed += Time.deltaTime;
            yield return null;
        }

        clone.transform.localScale = Vector3.one; // Ensure final scale is set to one
        // Open the door here
        if (doorAnimation != null)
        {
            doorAnimation.Play("microwavedooropen");
        }
        yield return new WaitForSeconds(doorAnimation.clip.length);
        cloneController.enabled = true; // Re-enable sheep controller

        yield return StartCoroutine(cloneController.RunToPoint(5f, initialSheepMovePoint));

        cloneController.Initialize();
        if (SheepReception.Instance.TrySendSheepToAvailablePen(cloneController, out var pen))
        {
            cloneController.SendToPen(pen);
        }
        else
        {
            // cloneController.Initialize();
        }

        yield return new WaitForSeconds(cloneDelay);
        if (doorAnimation != null)
        {
            doorAnimation.Play("microwavedoorclose");
        }
        isCloning = false;
    }

    void Update()
    {
        if (!isCloning) StartCoroutine(CloneSheep());
    }

}
