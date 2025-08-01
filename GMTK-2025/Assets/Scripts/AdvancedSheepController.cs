using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class AdvancedSheepController : MonoBehaviour, IShearable
{
    public bool looking = false;
    public bool moving = false;
    public float moveTimer = 0f;
    public bool isQueen = false;
    public AdvancedSheepController currentQueen = null;

    public LayerMask groundMask;
    public Transform playerTransform;

    public LayerMask sheapLayer;
    public LayerMask collisionLayer;

    public GameObject woolObject;

    public GameObject woolPrefab;
    public int woolColorIndex = 0; // Default color index
    public int woolSize = 1; // Default size

    private bool outOfRange = false;
    float currentMoveSpeed = 0f;

    bool lockMovement = false;
    bool isRunning = false;

    public bool IsSheared => isSheared;
    private bool isSheared = false;

    private static readonly Collider[] sheepBuffer = new Collider[16];
    private static readonly Collider[] obstacleBuffer = new Collider[16];

    private const int MAX_UPDATE_GROUP_COUNT = 256;
    private int myUpdateGroupCount = 0;
    private int currentUpdateGroupCount = 0;

    private const float SECONDS_BETWEEN_QUEEN_CHECKS = 2f;
    private float timeSinceLastQueenCheck = 0f;

    private const float RANGE_FOR_QUEEN_GROUPING = 100f;

    private const float MAX_MOVE_TIME = 5f;

    private Collider thisCollider;

    [SerializeField] private float[] lodDistances = new float[] { 50f, 150f, 300f, 500f, 1000f };
    [SerializeField] private int[] lodModules = new int[] { 1, 4, 16, 64, 256 };
    private int currentLOD = 0;

    void Awake()
    {
        thisCollider = GetComponent<Collider>();
    }

    void Start()
    {
        myUpdateGroupCount = UnityEngine.Random.Range(0, MAX_UPDATE_GROUP_COUNT);

        transform.position = GetGroundHeight(transform.position);
        DealWithQueen();
        StartCoroutine(RandomMoveSheep(true));
    }

    public void Reset()
    {
        moving = false;
        looking = false;
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
                new float2(bounds.min.x + 0.5f, bounds.max.z - 0.5f)  // Top Left
                );

                StartCoroutine(InPen(corners));
                // Debug.Log($"Sheep {gameObject.name} is in a pen at time {Time.frameCount}");
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, RANGE_FOR_QUEEN_GROUPING * 1.3f);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Sheep") && hit != thisCollider)
            {
                var sheep = hit.GetComponent<AdvancedSheepController>();
                if (sheep.isQueen)
                {
                    currentQueen = sheep;
                    return;
                }
            }
        }

        // If no queen found, become the queen
        isQueen = true;
    }

    private void FixedUpdate()
    {
        currentUpdateGroupCount++;

        currentLOD = Mathf.Clamp(currentLOD, 0, lodModules.Length - 1);

        int updateModulo = lodModules[currentLOD];
        if ((currentUpdateGroupCount % updateModulo) != (myUpdateGroupCount % updateModulo))
        {
            return;
        }

        // Determine the current LOD based on distance to the player
        {
            currentLOD = 0;
            while (currentLOD < lodDistances.Length - 1 && DistanceIgnoreY(transform.position, playerTransform.position) > lodDistances[currentLOD + 1])
            {
                currentLOD++;
            }
        }

        // Check if current queen is still valid
        if (currentQueen != null && !currentQueen.isQueen)
        {
            currentQueen = null;
        }

        // Handle sheep collision
        {
            int sheepCount = Physics.OverlapSphereNonAlloc(transform.position, 2f, sheepBuffer, sheapLayer);
            for (int i = 0; i < sheepCount; i++)
            {
                Collider col = sheepBuffer[i];

                // Fast tag check avoids GetComponent on non-sheep
                if (col != thisCollider && col.CompareTag("Sheep"))
                {
                    if (col.TryGetComponent<AdvancedSheepController>(out var sheep))
                    {
                        if (sheep != null)
                        {
                            Vector3 directionToSheep = sheep.transform.position - transform.position;
                            directionToSheep.y = 0;
                            directionToSheep.Normalize();
                            float distanceFromSheep = DistanceIgnoreY(transform.position, sheep.transform.position);

                            float escapeSpeed = 5f / distanceFromSheep;
                            escapeSpeed = Mathf.Clamp(escapeSpeed, 0.1f, 5f);
                            transform.position -= directionToSheep * Time.deltaTime * escapeSpeed;
                        }
                    }
                }
            }

        }

        // Handle object collision
        {
            int obstacleCount = Physics.OverlapSphereNonAlloc(transform.position, 1.5f, obstacleBuffer, groundMask | collisionLayer); // This value should change with the size of the sheep
            for (int i = 0; i < obstacleCount; i++)
            {
                if (obstacleBuffer[i].CompareTag("Obstacle"))
                {
                    Vector3 directionToObstacle = obstacleBuffer[i].transform.position - transform.position;
                    directionToObstacle.y = 0;
                    directionToObstacle.Normalize();

                    float escapeSpeed = currentMoveSpeed + 2f;
                    escapeSpeed = Mathf.Clamp(escapeSpeed, 0.1f, 5f);
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
        if (DistanceIgnoreY(transform.position, playerTransform.position) < 10f && !isRunning)
        {
            moving = false;
            looking = false;
            StopAllCoroutines();
            StartCoroutine(PanicSheep(playerTransform.position));
        }

        if (timeSinceLastQueenCheck >= SECONDS_BETWEEN_QUEEN_CHECKS)
        {
            // If is queen and another sheep is nearby, bring it to the flock
            if (isQueen)
            {
                Collider[] nearbyHits = Physics.OverlapSphere(transform.position, RANGE_FOR_QUEEN_GROUPING);
                foreach (var hit in nearbyHits)
                {
                    if (hit.CompareTag("Sheep") && hit != thisCollider)
                    {
                        var sheep = hit.GetComponent<AdvancedSheepController>();
                        if (!sheep.isQueen && sheep.currentQueen == null)
                        {
                            sheep.currentQueen = this;
                        }
                    }
                }
            }

            // If not queen, if sheep gets too far away, make it alone
            if (!isQueen && currentQueen != null)
            {
                float distanceToQueen = DistanceIgnoreY(transform.position, currentQueen.transform.position);
                if (distanceToQueen > RANGE_FOR_QUEEN_GROUPING * 1.5f)
                {
                    currentQueen = null;
                }
            }

            // I don't need to do this because other queens will handle it, keeping this here so I don't forget

            // If not queen and no current queen, check if it should become a queen
            // if (!isQueen && currentQueen == null)
            // {
            //     DealWithQueen();
            // }

            // If is queen and alone, make it alone
            if (isQueen)
            {
                bool isAlone = true;
                Collider[] nearbyHits = Physics.OverlapSphere(transform.position, RANGE_FOR_QUEEN_GROUPING);
                foreach (var hit in nearbyHits)
                {
                    if (hit.CompareTag("Sheep") && hit != thisCollider)
                    {
                        var sheep = hit.GetComponent<AdvancedSheepController>();
                        if (!sheep.isQueen)
                        {
                            isAlone = false;
                            break; // There are other sheep, so this one is not alone
                        }
                    }
                }
                // If no other sheep found, make it alone
                if (isAlone)
                {
                    isQueen = false;
                    currentQueen = null;
                }
            }
        }

        timeSinceLastQueenCheck += Time.deltaTime;
    }

    private IEnumerator RandomMoveSheep(bool recursive, float? angle = null)
    {
        moveTimer = 0f;
        if (isQueen || currentQueen == null)
        {
            if (isQueen && angle != null)
            {
                // Keep the queen moving in a similar direction
                angle = (float)angle + UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            else angle ??= UnityEngine.Random.Range(0f, Mathf.PI * 2f);

            // Pick a random location near a sheep
            float radius = UnityEngine.Random.Range(9f, 14f);

            // Pick a random speed
            float speed = UnityEngine.Random.Range(2f, 3f);
            currentMoveSpeed = speed;

            Vector3 targetPosition = transform.position + new Vector3(radius * Mathf.Cos((float)angle), 0, radius * Mathf.Sin((float)angle));
            targetPosition = GetGroundHeight(targetPosition);

            looking = true;
            // Rotate the sheep to face the target position
            while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position))) > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer > MAX_MOVE_TIME * 2f)
                {
                    // If the sheep has been moving for too long, stop moving
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position)), 180f * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            looking = false;

            moving = true;
            // Start moving towards the target position
            while (DistanceIgnoreY(transform.position, targetPosition) > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer > MAX_MOVE_TIME * 2f)
                {
                    // If the sheep has been moving for too long, stop moving
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            moving = false;
        }
        else
        {
            // Bias movement towards the queen
            Vector3 directionToQueen = (currentQueen.transform.position - transform.position).normalized;

            // Adjust the bias strength based on the distance to the queen
            float biasStrength = Mathf.Clamp01(Mathf.Pow(DistanceIgnoreY(transform.position, currentQueen.transform.position) / (RANGE_FOR_QUEEN_GROUPING), 2));

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

            looking = true;
            // Rotate the sheep to face the target position
            while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position))) > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer > MAX_MOVE_TIME)
                {
                    // If the sheep has been moving for too long, stop moving
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(targetPosition - transform.position)), 75f * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            looking = false;

            currentMoveSpeed = 5f; // This is for cases of pushing with other sheep

            moving = true;
            // Start moving towards the target position
            while (DistanceIgnoreY(transform.position, targetPosition) > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer > MAX_MOVE_TIME)
                {
                    // If the sheep has been moving for too long, stop moving
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            moving = false;
        }

        // If recursive, call the method again
        if (recursive)
        {
            float randomTime = UnityEngine.Random.Range(3f, 7f);
            yield return new WaitForSeconds(randomTime);
            StartCoroutine(RandomMoveSheep(true, isQueen ? angle : null));
        }
    }

    private IEnumerator PanicSheep(Vector3 playerPosition)
    {
        isRunning = true;

        // Panic mode: run away from the player
        Vector3 directionAwayFromPlayer = transform.position - playerPosition;
        directionAwayFromPlayer.y = 0; // Ignore vertical distance
        directionAwayFromPlayer.Normalize();

        float panicSpeed = UnityEngine.Random.Range(7f, 9f);
        currentMoveSpeed = panicSpeed;

        float panicDuration = UnityEngine.Random.Range(0.25f, 1.0f);
        float elapsedTime = 0f;

        while (elapsedTime < panicDuration)
        {
            // Move away from the player
            transform.position += directionAwayFromPlayer * panicSpeed * Time.deltaTime;

            // Slerp the rotation to face away from the player
            transform.SetPositionAndRotation(GetGroundHeight(transform.position), Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(transform.position - playerPosition)), 360f * Time.deltaTime));
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        isRunning = false;
        // After panic, resume normal movement
        StartCoroutine(RandomMoveSheep(true));
    }

    private IEnumerator FollowPosition(Transform position, Vector3 playerPosition, LassoController lasso)
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
                lasso.RemoveSheep(this);
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
        // Debug.Log("In pen coroutine started");
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

    public void GetLassoed(Transform target, Transform playerPosition, LassoController lasso)
    {
        StopAllCoroutines();
        moving = false;
        looking = false;
        isQueen = false;
        currentQueen = null;
        lockMovement = true;
        StartCoroutine(FollowPosition(target, playerPosition.position, lasso));
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
        // return position;

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

    public void Shear()
    {
        // Debug.Log("Shearing sheep: " + gameObject.name);
        if (isSheared) return;

        isSheared = true;
        woolObject.SetActive(false);

        GameObject woolInstance = Instantiate(woolPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
        woolInstance.GetComponentsInChildren<Pickuppable>()[0].woolSize = woolSize;
        // Debug.Log("Wool size: " + woolSize);
        woolInstance.GetComponentsInChildren<Pickuppable>()[0].woolColorIdx = woolColorIndex;
        Vector3 initialVelocity = new Vector3(UnityEngine.Random.Range(-.2f, 0.2f), 0.5f, UnityEngine.Random.Range(-0.2f, 0.2f)).normalized * 5f;
        woolInstance.GetComponent<Rigidbody>().AddForce(initialVelocity, ForceMode.Impulse);
    }
}
