using System.Collections;
using UnityEngine;

public class SimpleSheepMover : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(RandomMoveSheep(true));
    }

    public IEnumerator RandomMoveSheep(bool recursive)
    {
        // Wait random time before moving
        float randomTime = Random.Range(1f, 5f);
        yield return new WaitForSeconds(randomTime);

        // Pick a random location near a sheep
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(2f, 15f);

        // Pick a random speed
        float speed = Random.Range(3f, 4f);

        Vector3 targetPosition = transform.position + new Vector3(radius * Mathf.Cos(angle), 0, radius * Mathf.Sin(angle));

        // Start moving towards the target position
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        // If recursive, call the method again
        if (recursive)
        {
            StartCoroutine(RandomMoveSheep(true));
        }
    }
}
