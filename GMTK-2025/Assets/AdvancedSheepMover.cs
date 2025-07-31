using System.Collections;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class AdvancedSheepController : MonoBehaviour
{

    public bool isQueen = false;
    public AdvancedSheepController currentQueen = null;

    public LayerMask groundMask;
    public Transform playerTransform;

    private bool outOfRange = false;

    void Start()
    {
        DealWithQueen();
        StartCoroutine(RandomMoveSheep(true));
    }

    public void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(RandomMoveSheep(true));
    }

    private void DealWithQueen()
    {
        if (isQueen) return;

        // Search for a nearby queen
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 60f);
        foreach (var hit in hitColliders)
        {
            var sheep = hit.GetComponent<AdvancedSheepController>();
            if (sheep != null && sheep.isQueen)
            {
                currentQueen = sheep;
                return;
            }
        }

        // If no queen found, become the queen
        isQueen = true;
    }

    void Update()
    {
        // If the sheep is far from player, skip the update
        if (Vector3.Distance(transform.position, playerTransform.position) > 100f)
        {
            if (!outOfRange)
            {
                StopAllCoroutines();
                transform.position = GetGroundHeight(transform.position);
                outOfRange = true;
            }
            return;
        }
        else
        {
            if (outOfRange)
            {
                outOfRange = false;
                StartCoroutine(RandomMoveSheep(true));
            }
        }

        // Handle sheep collision
        {
            Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 2f); // This value should change with the size of the sheep
            foreach (var hit in nearbyHits)
            {
                var sheep = hit.GetComponent<AdvancedSheepController>();
                if (sheep != null && sheep != this)
                {
                    Vector3 directionToSheep = sheep.transform.position - transform.position;
                    directionToSheep.y = 0; // Ignore vertical distance
                    directionToSheep.Normalize();
                    float distanceFromSheep = DistanceIgnoreY(transform.position, sheep.transform.position);


                    // Move away from the other sheep
                    float escapeSpeed = 5f / distanceFromSheep;
                    escapeSpeed = Mathf.Clamp(escapeSpeed, 0.1f, 5f); // Limit speed to avoid too fast movement
                    transform.position -= directionToSheep * Time.deltaTime * escapeSpeed;
                }
            }
        }

        // Handle object collision
        {
            Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 1f); // This value should change with the size of the sheep
            foreach (var hit in nearbyHits)
            {
                if (hit.gameObject.CompareTag("Obstacle"))
                {
                    Vector3 directionToObstacle = hit.transform.position - transform.position;
                    directionToObstacle.y = 0; // Ignore vertical distance
                    directionToObstacle.Normalize();
                    float distanceFromObstacle = DistanceIgnoreY(transform.position, hit.transform.position);

                    // Move away from the obstacle
                    float escapeSpeed = 10f;
                    escapeSpeed = Mathf.Clamp(escapeSpeed, 0.1f, 5f); // Limit speed to avoid too fast movement
                    transform.position -= directionToObstacle * Time.deltaTime * escapeSpeed;
                }
            }
        }

        // If the player is close, enter panic mode
        if (DistanceIgnoreY(transform.position, playerTransform.position) < 7f)
        {
            StopAllCoroutines();
            StartCoroutine(PanicSheep(playerTransform.position));
        }

        // If is queen and another sheep is nearby, bring it to the flock
        if (isQueen)
        {
            Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 60f);
            foreach (var hit in nearbyHits)
            {
                var sheep = hit.GetComponent<AdvancedSheepController>();
                if (sheep != null && !sheep.isQueen && sheep.currentQueen == null)
                {
                    sheep.currentQueen = this;
                    sheep.isQueen = false;
                }
            }
        }

        // If not queen, if sheep gets too far away, make it alone
        if (!isQueen && currentQueen != null)
        {
            float distanceToQueen = DistanceIgnoreY(transform.position, currentQueen.transform.position);
            if (distanceToQueen > 80f)
            {
                currentQueen = null;
            }
        }

        // If is queen and alone, make it alone
        if (isQueen)
        {
            Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 60f);
            foreach (var hit in nearbyHits)
            {
                var sheep = hit.GetComponent<AdvancedSheepController>();
                if (sheep != null && !sheep.isQueen)
                {
                    continue; // There are other sheep, so this one is not alone
                }
            }
            // If no other sheep found, make it alone
            isQueen = false;
            currentQueen = null;
        }
    }

    private IEnumerator RandomMoveSheep(bool recursive, float? angle = null)
    {
        float currentMoveTime = 0f;
        if (isQueen || currentQueen == null)
        {
            // True random movement for the queen
            // Pick a random location near a sheep
            if (angle == null) angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(10f, 25f);

            // Pick a random speed
            float speed = Random.Range(5f, 6f);

            Vector3 targetPosition = transform.position + new Vector3(radius * Mathf.Cos((float)angle), 0, radius * Mathf.Sin((float)angle));
            targetPosition = GetGroundHeight(targetPosition);

            // Rotate the sheep to face the target position
            while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position)) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 180f * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            // Start moving towards the target position
            while (DistanceIgnoreY(transform.position, targetPosition) > 0.1f)
            {
                currentMoveTime += Time.deltaTime;
                if (currentMoveTime > 5f)
                {
                    // If the sheep has been moving for too long, stop moving
                    transform.position = GetGroundHeight(transform.position);
                    yield break;
                }
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
        }
        else
        {
            // Bias movement towards the queen
            Vector3 directionToQueen = (currentQueen.transform.position - transform.position).normalized;

            // Adjust the bias strength (0 = full random, 1 = always toward queen)
            float biasStrength = 0.3f;

            // Random offset in a circle
            if (angle == null) angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(2f, 15f);
            Vector3 randomOffset = new Vector3(radius * Mathf.Cos((float)angle), 0, radius * Mathf.Sin((float)angle));

            // Biased direction: mix between random offset and direction to queen
            Vector3 biasedOffset = Vector3.Lerp(randomOffset, directionToQueen * radius, biasStrength);

            // Pick a random speed
            float speed = Random.Range(3f, 4f);

            Vector3 targetPosition = transform.position + biasedOffset;
            targetPosition = GetGroundHeight(targetPosition);

            // Rotate the sheep to face the target position
            while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position)) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 75f * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            // Start moving towards the target position
            while (DistanceIgnoreY(transform.position, targetPosition) > 0.1f)
            {
                currentMoveTime += Time.deltaTime;
                if (currentMoveTime > 5f)
                {
                    // If the sheep has been moving for too long, stop moving
                    transform.position = GetGroundHeight(transform.position);
                    yield break;
                }
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
        }

        // If recursive, call the method again
        if (recursive)
        {
            float randomTime = Random.Range(3f, 7f);
            yield return new WaitForSeconds(randomTime);
            StartCoroutine(RandomMoveSheep(true));
        }
    }

    private IEnumerator PanicSheep(Vector3 playerPosition)
    {
        // Panic mode: run away from the player
        Vector3 directionAwayFromPlayer = transform.position - playerPosition;
        directionAwayFromPlayer.y = 0; // Ignore vertical distance
        directionAwayFromPlayer.Normalize();

        float panicSpeed = Random.Range(5f, 8f);

        float panicDuration = Random.Range(2f, 5f);
        float elapsedTime = 0f;

        while (elapsedTime < panicDuration)
        {
            // Move away from the player
            transform.position += directionAwayFromPlayer * panicSpeed * Time.deltaTime;

            // Slerp the rotation to face away from the player
            transform.SetPositionAndRotation(GetGroundHeight(transform.position), Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.position - playerPosition), 500f * Time.deltaTime));
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // After panic, resume normal movement
        StartCoroutine(RandomMoveSheep(true));
    }

    public void GetLassoed(Transform target)
    {
        StopAllCoroutines();
    }

    private float DistanceIgnoreY(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z));
    }

    // These functions are very expensive, so use them sparingly
    private Vector3 GetGroundHeight(Vector3 position)
    {
        Vector3 newPosition = position;
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f, layerMask: groundMask))
        {
            Debug.Log(hit.point.y);
            newPosition.y = hit.point.y;
            return newPosition;
        }
        return position; // Default to current height if no ground found
    }

    private float GetGroundHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out hit, 200f))
        {
            return hit.point.y;
        }
        return transform.position.y; // Default to current height if no ground found
    }
}
