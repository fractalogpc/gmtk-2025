using Unity.VisualScripting;
using UnityEngine;

public class LassoVisualController : MonoBehaviour
{
    public GameObject ropeStart;
    public GameObject ropeEnd;

    public GameObject ropePrefab;
    private GameObject rope;

    public Transform lassoOrigin;
    public Transform lassoGoal;

    private RopeController ropeController;
    public LassoLoopController lassoLoopController;

    bool onGround = false;
    bool isEnabled;

    void Update()
    {
        if (!isEnabled) return;
        ropeStart.transform.position = lassoOrigin.position + Vector3.down * 1.0f;
        ropeEnd.transform.position = lassoLoopController.ropeEnd.position;

        // Tighten the rope if on ground
        if (onGround || true)
        {
            float distance = Vector3.Distance(ropeStart.transform.position, ropeEnd.transform.position);
            int numberOfSegments = lassoLoopController.joints.Length;

            // Calculate the segment length based on the distance and number of segments
            float segmentLength = distance / numberOfSegments;
            segmentLength = Mathf.Max(segmentLength, 0.1f); // Ensure a minimum segment length
            segmentLength *= 0.7f; // Allow some slack

            ropeController.SetJointLengths(segmentLength);
        }
    }

    public void EnableVisual()
    {
        // Debug.Log("Resetting");
        rope = Instantiate(ropePrefab, transform.position, transform.rotation);

        ropeStart = rope.transform.GetChild(1).gameObject;
        ropeEnd = rope.transform.GetChild(10).gameObject;

        ropeController = rope.GetComponent<RopeController>();

        lassoLoopController = rope.GetComponentInChildren<LassoLoopController>();
        lassoLoopController.center = lassoGoal;
        lassoLoopController.CreatePrefabs();

        isEnabled = true;
    }

    public void HitGround()
    {
        onGround = true;
        lassoLoopController.onGround = true;
    }

    public void DisableVisual()
    {
        isEnabled = false;

        Destroy(rope);
    }

    public void StartPulling(bool isRetracting)
    {
        lassoLoopController.isPulling = isRetracting;
    }
}
