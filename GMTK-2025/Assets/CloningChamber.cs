using UnityEngine;
using System.Collections;

public class CloningChamber : MonoBehaviour
{

    [SerializeField] private GameObject sheepPrefab;

    [SerializeField] private AnimationCurve sizeCurve;
    [SerializeField] private float cloneDelay = 2f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform spinPlate;
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private Animation doorAnimation;
    [SerializeField] private float cloneDelayRandomAdd = 0.5f;
    [SerializeField] private Vector3 initialSheepMovePoint = new Vector3(0, 0, 0f);

    private ClonableSheep sheepBeingCloned;
    private bool isCloning = false;

    void Update()
    {
        if (sheepBeingCloned != null && !isCloning)
        {
            StartCoroutine(CloneSheep());
        }
    }

    public void ReplaceSheep(AdvancedSheepController sheepController)
    {
        if (sheepController == null) return;
        Debug.Log($"Replacing sheep in cloning chamber with: {sheepController.name}");
        ClonableSheep newSheep = new ClonableSheep
        {
            colorIndex = sheepController.woolColorIndex,
            size = sheepController.woolSize
        };

        sheepBeingCloned = newSheep;
    }

    private IEnumerator CloneSheep()
    {
        if (isCloning)
        {
            yield break; // Prevent multiple clones from being created simultaneously
        }

        isCloning = true;

        float randomDelay = Random.Range(0f, cloneDelayRandomAdd);
        yield return new WaitForSeconds(cloneDelay + randomDelay);
        ClonableSheep sheep = sheepBeingCloned;

        GameObject clone = Instantiate(sheepPrefab, spawnPoint.position, Quaternion.identity);
        clone.transform.name = "ClonedSheep";
        clone.transform.SetParent(GameObject.FindWithTag("SheepSpawner").transform);

        AdvancedSheepController cloneController = clone.GetComponent<AdvancedSheepController>();
        cloneController.PlayerTransform = GameObject.FindWithTag("Player").transform;
        cloneController.woolSize = sheep.size;
        cloneController.woolColorIndex = sheep.colorIndex;

        clone.transform.localScale = Vector3.zero; // Start with zero scale
        float elapsed = 0f;

        float cloneDuration = -1f;

        switch (sheep.colorIndex)
        {
            case 0:
            case 1:
            case 2:
                {
                    switch (sheep.size)
                    {
                        case 1:
                            cloneDuration = 25;
                            break;
                        case 2:
                            cloneDuration = 45;
                            break;
                        case 3:
                            cloneDuration = 60;
                            break;
                    }
                }
                break;
            case 3:
            case 4:
            case 5:
                {
                    switch (sheep.size)
                    {
                        case 1:
                            cloneDuration = 50;
                            break;
                        case 2:
                            cloneDuration = 90;
                            break;
                        case 3:
                            cloneDuration = 120;
                            break;
                    }
                }
                break;
        }

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
        if (doorAnimation != null)
        {
            doorAnimation.Play("microwavedoorclose");
        }
        isCloning = false;
    }
}

public class ClonableSheep
{
    public int colorIndex;
    public int size;
}
