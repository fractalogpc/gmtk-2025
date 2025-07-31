using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class AdvancedSheepController : MonoBehaviour
{

    public bool isQueen = false;
    public AdvancedSheepController currentQueen = null;

    public LayerMask groundMask;
    public Transform playerTransform;

    private bool outOfRange = false;
    float currentMoveSpeed = 0f;

    bool lockMovement = false;

    void Start()
    {
        DealWithQueen();
        StartCoroutine(RandomMoveSheep(true));
    }

    public void Reset()
    {
        StopAllCoroutines();
        // Check if in a pen
        Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 2f); // This value should change with the size of the sheep
        foreach (var hit in nearbyHits)
        {
            if (hit.CompareTag("Pen"))
            {
                var bounds = hit.GetComponent<Collider>().bounds;
                float2x4 corners = new float2x4(
                new float2(bounds.min.x + 0.5f, bounds.min.z + 0.5f), // Bottom Left
                new float2(bounds.max.x - 0.5f, bounds.min.z + 0.5f), // Bottom Right
                new float2(bounds.max.x - 0.5f, bounds.max.z - 0.5f), // Top Right
                new float2(bounds.min.x+ 0.5f, bounds.max.z - 0.5f)  // Top Left
                );

                StartCoroutine(InPen(corners));
                Debug.Log($"Sheep {gameObject.name} is in a pen at time {Time.frameCount}");
                return;
            }
        }

        lockMovement = false;

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
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > 100f)
            {
                if (!outOfRange)
                {
                    StopAllCoroutines();
                    transform.position = GetGroundHeight(transform.position);
                    outOfRange = true;
                    lockMovement = false;
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
            Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 1.5f); // This value should change with the size of the sheep
            foreach (var hit in nearbyHits)
            {
                if (hit.gameObject.CompareTag("Obstacle"))
                {
                    Vector3 directionToObstacle = hit.transform.position - transform.position;
                    directionToObstacle.y = 0; // Ignore vertical distance
                    directionToObstacle.Normalize();

                    // Move away from the obstacle
                    float escapeSpeed = currentMoveSpeed + 2f;
                    escapeSpeed = Mathf.Clamp(escapeSpeed, 0.1f, 5f); // Limit speed to avoid too fast movement
                    transform.position -= directionToObstacle * Time.deltaTime * escapeSpeed;
                }
            }
        }

        // If the sheep is lassoed, it should follow the lasso
        if (lockMovement)
        {
            return;
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
            if (angle == null) angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(10f, 25f);

            // Pick a random speed
            float speed = UnityEngine.Random.Range(5f, 6f);
            currentMoveSpeed = speed;

            Vector3 targetPosition = transform.position + new Vector3(radius * Mathf.Cos((float)angle), 0, radius * Mathf.Sin((float)angle));
            targetPosition = GetGroundHeight(targetPosition);

            // Rotate the sheep to face the target position
            while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position)) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position)), 180f * Time.deltaTime);
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
            if (angle == null) angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(2f, 15f);
            Vector3 randomOffset = new Vector3(radius * Mathf.Cos((float)angle), 0, radius * Mathf.Sin((float)angle));

            // Biased direction: mix between random offset and direction to queen
            Vector3 biasedOffset = Vector3.Lerp(randomOffset, directionToQueen * radius, biasStrength);

            // Pick a random speed
            float speed = UnityEngine.Random.Range(3f, 4f);
            currentMoveSpeed = speed;

            Vector3 targetPosition = transform.position + biasedOffset;
            targetPosition = GetGroundHeight(targetPosition);

            // Rotate the sheep to face the target position
            while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position)) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position)), 75f * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            currentMoveSpeed = 5f; // This is for cases of pushing with other sheep

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
            float randomTime = UnityEngine.Random.Range(3f, 7f);
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

        float panicSpeed = UnityEngine.Random.Range(5f, 8f);
        currentMoveSpeed = panicSpeed;

        float panicDuration = UnityEngine.Random.Range(2f, 5f);
        float elapsedTime = 0f;

        while (elapsedTime < panicDuration)
        {
            // Move away from the player
            transform.position += directionAwayFromPlayer * panicSpeed * Time.deltaTime;

            // Slerp the rotation to face away from the player
            transform.SetPositionAndRotation(GetGroundHeight(transform.position), Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(transform.position - playerPosition)), 500f * Time.deltaTime));
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // After panic, resume normal movement
        StartCoroutine(RandomMoveSheep(true));
    }

    private IEnumerator FollowPosition(Transform position, Vector3 playerPosition)
    {
        while (true)
        {
            if (position == null)
            {
                break;
            }
            float distanceToPosition = DistanceIgnoreY(transform.position, position.position);
            if (distanceToPosition > 5f)
            {
                // If the sheep is too far, break it out of the lasso
                lockMovement = false;
                Reset();
            }
            if (distanceToPosition > 2f)
            {
                float speed = Mathf.Clamp(Mathf.Pow(distanceToPosition, 2), 0f, 10f); // Speed increases with distance
                transform.position = Vector3.MoveTowards(transform.position, position.position, speed * Time.deltaTime);
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(position.position - transform.position)), 500f * Time.deltaTime);
            yield return null; // Wait for the next frame
        }
    }

    private IEnumerator InPen(float2x4 corners)
    {
        Debug.Log("In pen coroutine started");
        Vector2 center = new Vector2(transform.position.x, transform.position.z);

        // Convert float2x4 to Vector2[]
        Vector2[] penCorners = new Vector2[4]
        {
    corners.c0,
    corners.c1,
    corners.c2,
    corners.c3
        };

        Vector2 targetPoint2D;
        int attempts = 0;
        do
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(1f, 5f);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            targetPoint2D = center + offset;
            attempts++;
            if (attempts > 100) yield break; // Prevent infinite loop
        }
        while (!PointInQuad(targetPoint2D, penCorners));


        Vector3 targetPosition = new Vector3(targetPoint2D.x, transform.position.y, targetPoint2D.y);
        targetPosition = GetGroundHeight(targetPosition);
        float speed = UnityEngine.Random.Range(3f, 4f);

        // Rotate the sheep to face the target position
        while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position)) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position)), 180f * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        float randomTime = UnityEngine.Random.Range(4f, 7f);
        yield return new WaitForSeconds(randomTime);
        StartCoroutine(InPen(corners));
    }

    public void GetLassoed(Transform target, Transform playerPosition)
    {
        StopAllCoroutines();
        isQueen = false;
        currentQueen = null;
        lockMovement = true;
        StartCoroutine(FollowPosition(target, playerPosition.position));
    }

    private float DistanceIgnoreY(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z));
    }

    private Vector3 IgnoreY(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    // Simple quad check using cross products (convex quads only)
    bool PointInQuad(Vector2 p, Vector2[] quad)
    {
        return PointInTriangle(p, quad[0], quad[1], quad[2]) ||
               PointInTriangle(p, quad[0], quad[2], quad[3]);
    }

    // Barycentric check for triangle
    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v0 = c - a;
        Vector2 v1 = b - a;
        Vector2 v2 = p - a;

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float denom = dot00 * dot11 - dot01 * dot01;
        if (Mathf.Abs(denom) < 0.0001f)
            return false;

        float u = (dot11 * dot02 - dot01 * dot12) / denom;
        float v = (dot00 * dot12 - dot01 * dot02) / denom;

        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }

    // These functions are very expensive, so use them sparingly
    private Vector3 GetGroundHeight(Vector3 position)
    {
        Vector3 newPosition = position;
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f, layerMask: groundMask))
        {
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
