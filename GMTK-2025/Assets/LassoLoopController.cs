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

    public bool onGround = false;
    public bool isPulling = false;

    private Transform playerTransform;

    void Start()
    {
        playerTransform = FindFirstObjectByType<PlayerController>().transform;
    }


    public void CreatePrefabs()
    {
        joints = new Transform[jointCount];
        for (int i = 0; i < jointCount; i++)
        {
            float angle = i * Mathf.PI * 2f / jointCount;
            Vector3 position = center.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Transform joint = Instantiate(jointPrefab, position, Quaternion.identity).transform;
            joint.SetParent(transform);
            joints[i] = joint;
        }

        ropeEnd = joints[0];

        GetComponent<RopeMesher>().SetSegments(joints);
    }

    public float rotationSpeed = 180f; // degrees per second
    float timer;
    float rotation;

    void Update()
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
                // Rotate towards the player at a fixed speed
                float angleToPlayer = Mathf.Atan2(directionToPlayer.z, directionToPlayer.x) * Mathf.Rad2Deg;
                rotation = Mathf.MoveTowardsAngle(rotation, angleToPlayer, 180f * Time.deltaTime);
            }
        }

        // Update the positions of the joints to form a rotating loop
        for (int i = 0; i < jointCount; i++)
        {
            // Rotate around the center
            float angle = i * Mathf.PI * 2f / jointCount + rotation * Mathf.Deg2Rad;
            Vector3 position = center.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            joints[i].position = position;
        }

        radius = sizeCurve.Evaluate(timer);
    }
}
