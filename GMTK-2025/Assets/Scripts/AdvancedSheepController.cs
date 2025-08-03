using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using FMODUnity;
using System.Collections.Generic;

public class AdvancedSheepController : MonoBehaviour, IShearable
{
    public bool isDeactivatedOnStart = false;
    private bool isActivated = true;

    public bool looking = false;
    public bool moving = false;
    public bool running = false;
    public float moveTimer = 0f;
    public bool isQueen = false;
    public AdvancedSheepController currentQueen = null;
    [SerializeField] private GameObject interactableObject;

    private SheepAudio sheepAudio;

    public LayerMask groundMask;
    public Transform PlayerTransform
    {
        set
        {
            playerTransform = value;
            sheepAnimation.playerPosition = value;
        }
    }
    private Transform playerTransform;

    public LayerMask sheapLayer;
    public LayerMask collisionLayer;

    public GameObject[] woolObjects;
    public GameObject[] tailObjects;

    public GameObject woolPrefab;
    public int woolColorIndex = 0; // Default color index
    public int woolSize = 1; // Default size

    public Collider[] colliders;
    public GameObject inCartCollider;

    bool lockMovement = false;
    bool isRunning = false;

    public bool IsSheared => isSheared;
    private bool isSheared = false;

    private static readonly Collider[] sheepBuffer = new Collider[16];
    private static readonly Collider[] obstacleBuffer = new Collider[16];

    private const int MAX_UPDATE_GROUP_COUNT = 256;
    private int myUpdateGroupCount = 0;
    private int currentUpdateGroupCount = 0;

    private const float SECONDS_BETWEEN_QUEEN_CHECKS = 10f;
    private float timeSinceLastQueenCheck = 0f;

    private const float RANGE_FOR_QUEEN_GROUPING = 100f;

    private const float MAX_MOVE_TIME = 5f;

    private const float ROTATION_SPEED = 90f; // Degrees per second

    private Collider thisCollider;

    private SheepAnimation sheepAnimation;

    public StudioEventEmitter woolPopSoundEmitter;

    [SerializeField] private float[] lodDistances = new float[] { 50f, 150f, 300f, 500f, 1000f };
    [SerializeField] private int[] lodModules = new int[] { 1, 4, 16, 64, 256 };
    private int currentLOD = 0;

    public Renderer[] renderers;

    [HideInInspector] public Material woolMaterial;

    void Awake()
    {
        if (isDeactivatedOnStart)
        {
            isActivated = false;
        }

        thisCollider = GetComponent<Collider>();

        sheepAnimation = GetComponentInChildren<SheepAnimation>();

        sheepAudio = GetComponentInChildren<SheepAudio>();
    }

    void Start()
    {
        if (isDeactivatedOnStart) return;

        myUpdateGroupCount = UnityEngine.Random.Range(0, MAX_UPDATE_GROUP_COUNT);
        currentLOD = Mathf.Clamp(currentLOD, 0, lodModules.Length - 1);

        transform.position = GetGroundHeight(transform.position);
        DealWithQueen();
        StartCoroutine(RandomMoveSheep(true));

        woolMaterial = woolObjects[0].GetComponent<Renderer>().material;

        foreach (var rend in tailObjects)
        {
            rend.GetComponent<Renderer>().material = woolMaterial;
        }
    }

    public void Initialize()
    {
        myUpdateGroupCount = UnityEngine.Random.Range(0, MAX_UPDATE_GROUP_COUNT);
        currentLOD = Mathf.Clamp(currentLOD, 0, lodModules.Length - 1);

        transform.position = GetGroundHeight(transform.position);
        DealWithQueen();
        StartCoroutine(RandomMoveSheep(true));

        woolMaterial = woolObjects[0].GetComponent<Renderer>().material;

        foreach (var rend in tailObjects)
        {
            rend.GetComponent<Renderer>().material = woolMaterial;
        }

        SheepSpawner.Instance.AddSheep(this);

        isActivated = true;
    }

    bool skippingFrame = false;

    public void Reset()
    {
        moving = false;
        looking = false;
        isHeld = false;
        Show();
        lockMovement = false;
        isRunning = false;
        moveTimer = 0f;
        StopAllCoroutines();
        // Check if in a pen
        // Collider[] nearbyHits = Physics.OverlapSphere(transform.position, 2f); // This value should change with the size of the sheep
        // foreach (var hit in nearbyHits)
        // {
        //     if (hit.CompareTag("Pen"))
        //     {
        //         var bounds = hit.GetComponent<Collider>().bounds;
        //         float2x4 corners = new float2x4(
        //         new float2(bounds.min.x + 0.5f, bounds.min.z + 0.5f), // Bottom Left
        //         new float2(bounds.max.x - 0.5f, bounds.min.z + 0.5f), // Bottom Right
        //         new float2(bounds.max.x - 0.5f, bounds.max.z - 0.5f), // Top Right
        //         new float2(bounds.min.x + 0.5f, bounds.max.z - 0.5f)  // Top Left
        //         );

        //         StartCoroutine(InPen(corners));
        //         // Debug.Log($"Sheep {gameObject.name} is in a pen at time {Time.frameCount}");
        //         return;
        //     }
        // }

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

    public void ManualUpdate()
    {
        if (!isActivated) return;
        if (inCart) return;
        if (isHeld)
        {
            // If the sheep is held, move it to the held position
            transform.position = heldPosition.position;
            transform.rotation = heldPosition.rotation;
        }
        // }

        // private void FixedUpdate()
        // {
        currentUpdateGroupCount++;

        currentLOD = Mathf.Clamp(currentLOD, 0, lodModules.Length - 1);

        int updateModulo = lodModules[currentLOD];
        if ((currentUpdateGroupCount % updateModulo) != (myUpdateGroupCount % updateModulo))
        {
            skippingFrame = true;
            return;
        }
        else
        {
            skippingFrame = false;
        }

        // Determine the current LOD based on distance to the player
        {
            currentLOD = 0;
            while (currentLOD < lodDistances.Length - 1 && Vector3.Distance(transform.position, playerTransform.position) > lodDistances[currentLOD + 1])
            {
                currentLOD++;
            }
        }

        // Handle animations
        sheepAnimation.TestForAnimation();

        // Handle audio
        sheepAudio.UpdateSound();

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
        // Obstacle avoidance using ComputePenetration
        {
            int obstacleCount = Physics.OverlapSphereNonAlloc(
              transform.position,
              1.5f,
              obstacleBuffer,
              collisionLayer
            );

            Vector3 totalOffset = Vector3.zero;

            for (int i = 0; i < obstacleCount; i++)
            {
                Collider obstacle = obstacleBuffer[i];

                // Skip self
                if (obstacle == thisCollider) continue;

                if (Physics.ComputePenetration(
                  thisCollider, transform.position, transform.rotation,
                  obstacle, obstacle.transform.position, obstacle.transform.rotation,
                  out Vector3 direction, out float distance))
                {
                    // Only apply horizontal offset
                    direction.y = 0;
                    Vector3 offset = direction.normalized * distance;
                    totalOffset += offset;
                }
            }

            if (totalOffset != Vector3.zero)
            {
                transform.position += totalOffset;

                // Optional: sync physics if using Rigidbody with interpolation
                // Physics.SyncTransforms();
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
            if (InventoryController.Instance.IsHoldingObject(InventoryController.ItemType.Shears))
            {
                moving = false;
                looking = false;
                StopAllCoroutines();
                StartCoroutine(PanicSheep(playerTransform.position));
            }
        }

        if (timeSinceLastQueenCheck >= SECONDS_BETWEEN_QUEEN_CHECKS)
        {
            timeSinceLastQueenCheck = 0f;

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

    public void SendToPen(Pen.SubPen pen, bool skipFirstPoint = false)
    {
        StopAllCoroutines();

        Debug.Log("Sheep going to pen");

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        StartCoroutine(RunToPen(pen, skipFirstPoint));
    }

    public IEnumerator RandomMoveSheep(bool recursive, float? angle = null)
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

            Vector3 targetPosition = transform.position + new Vector3(radius * Mathf.Cos((float)angle), 0, radius * Mathf.Sin((float)angle));
            targetPosition = GetGroundHeight(targetPosition);

            looking = true;
            Quaternion initialRotation = transform.rotation;
            Quaternion finalRotation = Quaternion.LookRotation(IgnoreY(targetPosition - transform.position));

            float angleToRotate = Quaternion.Angle(initialRotation, finalRotation);

            float rotationDuration = angleToRotate / ROTATION_SPEED;
            float rotationElapsed = 0f;

            while (Quaternion.Angle(transform.rotation, finalRotation) > 0.1f)
            {

                float skippedTime = 0f;
                // Wait for the next frame if the update group count does not match
                while (skippingFrame)
                {
                    skippedTime += Time.deltaTime;
                    moveTimer += Time.deltaTime;
                    rotationElapsed += Time.deltaTime;
                    yield return null; // Wait for the next frame
                }
                // if ((currentUpdateGroupCount % updateModulo) != (myUpdateGroupCount % updateModulo))
                // {
                //     yield return null; // Wait for the next frame
                // }

                if (moveTimer > MAX_MOVE_TIME * 2f)
                {
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }

                finalRotation = Quaternion.LookRotation(IgnoreY(targetPosition - transform.position));
                angleToRotate = Quaternion.Angle(initialRotation, finalRotation);
                rotationDuration = Mathf.Max(angleToRotate / ROTATION_SPEED, 0.01f); // Avoid division by zero

                // Add both skipped time and current frame’s deltaTime
                float currentDelta = Time.deltaTime;
                float totalDelta = skippedTime + currentDelta;
                rotationElapsed += totalDelta;

                float t = Mathf.Clamp01(rotationElapsed / rotationDuration);
                float easedT = Mathf.SmoothStep(0f, 1f, t);
                transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, easedT);

                yield return null;
            }

            looking = false;

            moving = true;
            // Start moving towards the target position
            while (DistanceIgnoreY(transform.position, targetPosition) > 0.1f)
            {
                float skippedTime = 0f;

                // Accumulate skipped time while this sheep is waiting its turn
                while (skippingFrame)
                {
                    float delta = Time.deltaTime;
                    skippedTime += delta;
                    moveTimer += delta;
                    rotationElapsed += delta;
                    yield return null;
                }

                float deltaTime = Time.deltaTime;
                float totalDelta = skippedTime + deltaTime;

                moveTimer += totalDelta;
                if (moveTimer > MAX_MOVE_TIME * 2f)
                {
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }

                // Move based on total actual time passed
                transform.position = Vector3.MoveTowards(
                  transform.position,
                  targetPosition,
                  speed * totalDelta
                );

                yield return null;
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

            Vector3 targetPosition = transform.position + biasedOffset;
            targetPosition = GetGroundHeight(targetPosition);

            looking = true;

            Quaternion initialRotation = transform.rotation;
            Quaternion finalRotation = Quaternion.LookRotation(IgnoreY(targetPosition - transform.position));

            float angleToRotate = Quaternion.Angle(initialRotation, finalRotation);

            float rotationDuration = angleToRotate / ROTATION_SPEED;
            float rotationElapsed = 0f;

            while (Quaternion.Angle(transform.rotation, finalRotation) > 0.1f)
            {
                float skippedTime = 0f;
                // Wait for the next frame if the update group count does not match
                while (skippingFrame)
                {
                    skippedTime += Time.deltaTime;
                    moveTimer += Time.deltaTime;
                    rotationElapsed += Time.deltaTime;
                    yield return null; // Wait for the next frame
                }

                if (moveTimer > MAX_MOVE_TIME * 2f)
                {
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }

                finalRotation = Quaternion.LookRotation(IgnoreY(targetPosition - transform.position));
                angleToRotate = Quaternion.Angle(initialRotation, finalRotation);
                rotationDuration = Mathf.Max(angleToRotate / ROTATION_SPEED, 0.01f); // Avoid division by zero

                // Add both skipped time and current frame’s deltaTime
                float currentDelta = Time.deltaTime;
                float totalDelta = skippedTime + currentDelta;
                rotationElapsed += totalDelta;

                float t = Mathf.Clamp01(rotationElapsed / rotationDuration);
                float easedT = Mathf.SmoothStep(0f, 1f, t);
                transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, easedT);

                yield return null;
            }

            looking = false;

            moving = true;
            while (DistanceIgnoreY(transform.position, targetPosition) > 0.1f)
            {
                float skippedTime = 0f;

                // Accumulate skipped time while this sheep is waiting its turn
                while (skippingFrame)
                {
                    float delta = Time.deltaTime;
                    skippedTime += delta;
                    moveTimer += delta;
                    rotationElapsed += delta;
                    yield return null;
                }

                float deltaTime = Time.deltaTime;
                float totalDelta = skippedTime + deltaTime;

                moveTimer += totalDelta;
                if (moveTimer > MAX_MOVE_TIME * 2f)
                {
                    transform.position = GetGroundHeight(transform.position);
                    break;
                }

                // Move based on total actual time passed
                transform.position = Vector3.MoveTowards(
                  transform.position,
                  targetPosition,
                  speed * totalDelta
                );

                yield return null;
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
        moving = true;
        running = true;

        // Panic mode: run away from the player
        Vector3 directionAwayFromPlayer = transform.position - playerPosition;
        directionAwayFromPlayer.y = 0; // Ignore vertical distance
        directionAwayFromPlayer.Normalize();

        float panicSpeed = UnityEngine.Random.Range(7f, 9f);

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

        moving = false;
        running = false;
        // After panic, resume normal movement
        StartCoroutine(RandomMoveSheep(true));
    }

    private IEnumerator FollowPosition(Transform position, Vector3 playerPosition, LassoController lasso)
    {
        moving = true;
        while (true)
        {
            if (position == null)
            {
                break;
            }
            float distanceToPosition = DistanceIgnoreY(transform.position, position.position);
            // if (distanceToPosition > 5f)
            // {
            //     // If the sheep is too far, break it out of the lasso
            //     lockMovement = false;
            //     lasso.RemoveSheep(this);
            //     Reset();
            // }
            if (distanceToPosition > 2f)
            {
                float speed = Mathf.Clamp(Mathf.Pow(distanceToPosition, 2), 0f, 10f); // Speed increases with distance
                transform.position = Vector3.MoveTowards(transform.position, position.position, speed * Time.deltaTime);
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(position.position - transform.position)), 500f * Time.deltaTime);
            yield return null; // Wait for the next frame
        }
        moving = false;
    }

    private IEnumerator RunToPen(Pen.SubPen pen, bool skipFirstPoint = false)
    {
        Vector3[] allPositions = System.Array.ConvertAll(SheepReception.Instance.pathingPoints, t => t.position);
        int[] indices = pen.pathingIndices;

        Vector3[] selectedPositions = new Vector3[indices.Length];
        for (int i = skipFirstPoint ? 2 : 0; i < indices.Length; i++)
        {
            selectedPositions[i] = allPositions[indices[i]];
        }

        float speed = UnityEngine.Random.Range(5f, 9f);

        for (int i = 0; i < selectedPositions.Length; i++)
        {
            yield return StartCoroutine(RunToPoint(speed, selectedPositions[i]));
        }

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, 2.0f));

        inPen = true;

        // Make sheep interactable now that it's in the pen
        interactableObject.layer = LayerMask.NameToLayer("Interactable");

        StartCoroutine(InPen(pen));
    }

    public IEnumerator FollowPoints(List<Vector3> points, float speed)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, 4.0f)); // Ensure the coroutine starts after the frame ends
        transform.position = points[0];
        while (points.Count > 0)
        {
            Vector3 targetPoint = points[0];
            yield return StartCoroutine(RunToPoint(speed, targetPoint));

            points.RemoveAt(0);
        }

        Destroy(gameObject); // Destroy the sheep after following all points
    }

    public IEnumerator RunToPoint(float speed, Vector3 point)
    {
        while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(IgnoreY(point - transform.position))) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(IgnoreY(point - transform.position)), 300f * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        moving = true;
        while (DistanceIgnoreY(transform.position, point) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);
            yield return null;
        }
        moving = false;
    }

    private bool inPen = false;
    public bool InPenValue { get { return inPen; } }
    private IEnumerator InPen(Pen.SubPen pen)
    {
        Vector2 center = new Vector2(transform.position.x, transform.position.z);

        float2x4 corners = pen.GetCorners(); // Recalculate because bounds might have changed

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
        // targetPosition = GetGroundHeight(targetPosition);
        float speed = UnityEngine.Random.Range(3f, 4f);

        // Rotate the sheep to face the target position
        Quaternion initialRotation = transform.rotation;
        Quaternion finalRotation = Quaternion.LookRotation(IgnoreY(targetPosition - transform.position));

        float angleToRotate = Quaternion.Angle(initialRotation, finalRotation);

        float rotationDuration = angleToRotate / ROTATION_SPEED;
        float rotationElapsed = 0f;

        while (Quaternion.Angle(transform.rotation, finalRotation) > 0.1f)
        {
            moveTimer += Time.deltaTime;
            rotationElapsed += Time.deltaTime;

            if (moveTimer > MAX_MOVE_TIME * 2f)
            {
                // transform.position = GetGroundHeight(transform.position);
                break;
            }

            finalRotation = Quaternion.LookRotation(IgnoreY(targetPosition - transform.position));
            angleToRotate = Quaternion.Angle(initialRotation, finalRotation);
            rotationDuration = Mathf.Max(angleToRotate / ROTATION_SPEED, 0.01f); // Avoid division by zero

            float t = Mathf.Clamp01(rotationElapsed / rotationDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, easedT);

            yield return null;
        }

        moving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        moving = false;

        float randomTime = UnityEngine.Random.Range(4f, 7f);
        yield return new WaitForSeconds(randomTime);
        StartCoroutine(InPen(pen));
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

    public void Shear(bool doubleShear = false)
    {
        // Debug.Log("Shearing sheep: " + gameObject.name);
        if (isSheared) return;

        isSheared = true;
        foreach (GameObject obj in woolObjects)
        {
            obj.SetActive(false);
        }

        SpawnWool();
        if (doubleShear)
        {
            SpawnWool(); // Spawn an additional wool instance if double shear is true
        }

    }

    private void SpawnWool()
    {
        GameObject woolInstance = Instantiate(woolPrefab, transform.position + Vector3.up * 0.5f, transform.rotation);
        Renderer renderer = woolInstance.GetComponentInChildren<Renderer>();
        renderer.material = woolMaterial; // Use the first wool material as a base
        woolInstance.GetComponentsInChildren<Pickuppable>()[0].woolSize = woolSize;
        // Debug.Log("Wool size: " + woolSize);
        woolInstance.GetComponentsInChildren<Pickuppable>()[0].woolColorIdx = woolColorIndex;
        Vector3 initialVelocity = new Vector3(UnityEngine.Random.Range(-.2f, 0.2f), 0.5f, UnityEngine.Random.Range(-0.2f, 0.2f)).normalized * 5f;
        woolInstance.GetComponent<Rigidbody>().AddForce(initialVelocity, ForceMode.Impulse);
        woolPopSoundEmitter.Play();
    }

    private bool isHeld = false;
    private Transform heldPosition;
    private Collider[] collidersToDeactivate;
    public void TryPickup()
    {
        if (!inPen) return;

        if (isSheared) return;

        if (InventoryController.Instance.GetNextAvailableSlot(out int index))
        {
            // Sheep is picked up
            collidersToDeactivate = GetComponentsInChildren<Collider>();
            foreach (Collider col in collidersToDeactivate)
            {
                col.enabled = false;
            }

            // SheepSpawner.Instance.RemoveSheep(this);

            sheepAnimation.enabled = false;
            StopAllCoroutines();
            InventoryController.Instance.TryAddItem(InventoryController.ItemType.Sheep, index, sheep: this);

            isHeld = true;
            heldPosition = ToolController.Instance.sheepHoldPosition;
            transform.localScale *= heldPosition.localScale.x; // Scale the sheep to match the held position
            woolPopSoundEmitter.Play();
        }
    }

    public void Show()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    public void Hide()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    private bool inCart = false;
    public void PutInCart(CartController cartController)
    {
        Debug.Log("Putting sheep in cart: " + gameObject.name);
        StopAllCoroutines();
        inCartCollider.SetActive(true);
        inCartCollider.GetComponentInParent<GenericInteractable>().OnInteract.AddListener(() =>
        {
            cartController.TryPlaceSheep();
        });

        inCart = true;
    }
}
