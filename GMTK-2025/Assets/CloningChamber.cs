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
        clone.GetComponent<AdvancedSheepController>().enabled = false; // Disable sheep controller during cloning
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
                                                  // Remove sheep from spin plate
        clone.GetComponent<AdvancedSheepController>().enabled = true; // Re-enable sheep controller
        // Let sheep go into pen idk how
        yield return new WaitForSeconds(cloneDelay);
        isCloning = false;
    }

    void Update()
    {
        if (!isCloning) StartCoroutine(CloneSheep());
    }

}
