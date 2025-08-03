using System.Collections.Generic;
using Player;
using UnityEngine;

public class LassoLoopController : MonoBehaviour
{
    public Transform center;
    public GameObject jointPrefab;
    public int jointCount = 10;
    public float radius = 0f;

    public Transform ropeEnd;

    public Transform[] joints;

    public AnimationCurve sizeCurve;
    public float sizeCurveMult = 1f;

    public bool onGround = false;
    public bool isPulling = false;

    private Transform playerTransform;

    private List<Transform> lassoedSheep = new List<Transform>();
    public float tightenSpeed = 5f;
    public float sheepWidth = 0.5f;

    public float maxLiftHeight = 2f;       // max height joint can lift
    public float liftDistanceThreshold = 3f; // max distance to sheep to start lifting

    private float globalRotation = 0f;
    public float rotationSpeed = 180f; // degrees per second

    void Start()
    {
        playerTransform = FindFirstObjectByType<PlayerController>().transform;
    }


    public void CreatePrefabs()
    {
        joints = new Transform[jointCount];
        jointAngles = new float[jointCount];
        jointRadii = new float[jointCount];

        for (int i = 0; i < jointCount; i++)
        {
            float angle = i * Mathf.PI * 2f / jointCount;
            jointAngles[i] = angle; // Store angle
            jointRadii[i] = radius; // Start at full radius

            Vector3 position = center.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Transform joint = Instantiate(jointPrefab, position, Quaternion.identity).transform;
            joint.SetParent(transform);
            joints[i] = joint;
        }


        ropeEnd = joints[0];

        GetComponent<RopeMesher>().SetSegments(joints);
    }

    float timer;
    float rotation;

    private float[] jointAngles;
    private float[] jointRadii;


    void LateUpdate()
    {
        if (joints == null || joints.Length < 2) return;

        // Increment rotation based on speed and time
        if (!onGround)
        {
            timer += Time.deltaTime;
            rotation += rotationSpeed * Time.deltaTime;
        }
        else
        {
            // If pulling, rotate towards the player
            if (isPulling)
            {
                Vector3 directionToPlayer = (playerTransform.position - center.position).normalized;
                float angleToPlayer = Mathf.Atan2(directionToPlayer.z, directionToPlayer.x) * Mathf.Rad2Deg;
                globalRotation = Mathf.MoveTowardsAngle(globalRotation, angleToPlayer, rotationSpeed * Time.deltaTime);
            }
        }

        if (!onGround)
        {
            // Update the positions of the joints to form a rotating loop
            for (int i = 0; i < jointCount; i++)
            {
                // Rotate around the center
                float angle = i * Mathf.PI * 2f / jointCount + rotation * Mathf.Deg2Rad;
                Vector3 position = center.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                joints[i].position = position;
            }

            radius = sizeCurve.Evaluate(timer) * sizeCurveMult;
        }
        else
        {
            // Constrict towards the sheep
            if (lassoedSheep == null) return;

            for (int i = 0; i < jointCount; i++)
            {
                Vector3 centerPos = center.position;

                float angle = jointAngles[i] + globalRotation * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                float targetDistance = 0f;
                bool blocked = false;

                foreach (var sheep in lassoedSheep)
                {
                    Vector3 sheepPos = sheep.position;
                    Vector3 toSheep = sheepPos - centerPos;

                    float projection = Vector3.Dot(toSheep, dir);
                    if (projection < 0f || projection > 20f)
                        continue;

                    Vector3 closestPoint = centerPos + dir * projection;
                    float distanceToLine = (sheepPos - closestPoint).magnitude;

                    if (distanceToLine < sheepWidth)
                    {
                        float backoff = Mathf.Sqrt(sheepWidth * sheepWidth - distanceToLine * distanceToLine);
                        float edgeDistance = projection + backoff;
                        targetDistance = Mathf.Max(targetDistance, edgeDistance);
                        blocked = true;
                    }

                    if (!blocked)
                        targetDistance = 0f; // No sheep in path — collapse fully to center
                }

                jointRadii[i] = Mathf.MoveTowards(jointRadii[i], targetDistance, tightenSpeed * Time.deltaTime);
                joints[i].position = centerPos + dir * jointRadii[i];

                if (lassoedSheep.Count > 0)
                {
                    // If there are sheep, move the rope up
                    joints[i].position += Vector3.up * 1f;
                }

                Debug.DrawLine(joints[i].position, joints[i].position + dir * jointRadii[i], Color.green);
                Debug.DrawRay(centerPos, dir * 20f, Color.yellow);

            }

        }

    }

    public void LassoedSheep(AdvancedSheepController[] sheep)
    {
        for (int i = 0; i < jointCount; i++)
        {
            jointRadii[i] = radius;
        }

        lassoedSheep.Clear();
        for (int i = 0; i < sheep.Length; i++)
        {
            lassoedSheep.Add(sheep[i].transform);
        }
    }

    public void ReleasePoints(Transform[] sheepToRemove)
    {
        foreach (var sheep in sheepToRemove)
        {
            if (lassoedSheep.Contains(sheep))
            {
                lassoedSheep.Remove(sheep);
            }
        }
    }

    public void Upgrade1()
    {
        sizeCurveMult = 1.5f; // Increase the size curve multiplier for upgrade 1
    }

    public void Upgrade2()
    {
        sizeCurveMult = 2f; // Increase the size curve multiplier for upgrade 2
    }

    public void Upgrade3()
    {
        sizeCurveMult = 4f; // Increase the size curve multiplier for upgrade 3
    }

}
