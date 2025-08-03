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

        // Tighten the rope if on ground
        if (onGround || true)
        {
            float distance = Vector3.Distance(ropeStart.transform.position, ropeEnd.transform.position);
            int numberOfSegments = lassoLoopController.joints.Length;

            // Calculate the segment length based on the distance and number of segments
            float segmentLength = distance / numberOfSegments;
            segmentLength = Mathf.Max(segmentLength, 0.1f); // Ensure a minimum segment length
            segmentLength *= 0.3f; // Allow some slack

            ropeController.SetJointLengths(segmentLength);
        }
    }

    void LateUpdate()
    {
        if (!isEnabled) return;

        ropeStart.transform.position = lassoOrigin.position + Vector3.down * 0.5f;
        ropeEnd.transform.position = lassoLoopController.ropeEnd.position;
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

        if (upgrade1)
            lassoLoopController.Upgrade1();
        if (upgrade2)
            lassoLoopController.Upgrade2();
        if (upgrade3)
            lassoLoopController.Upgrade3();

        isEnabled = true;
    }

    public void LassoedSheep(AdvancedSheepController[] sheep)
    {
        lassoLoopController.LassoedSheep(sheep);
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

    bool upgrade1 = false;
    bool upgrade2 = false;
    bool upgrade3 = false;

    public void Upgrade1()
    {
        upgrade1 = true;
        if (lassoLoopController != null)
            lassoLoopController.Upgrade1();
    }

    public void Upgrade2()
    {
        upgrade2 = true;
        if (lassoLoopController != null)
            lassoLoopController.Upgrade1();
    }

    public void Upgrade3()
    {
        upgrade3 = true;
        if (lassoLoopController != null)
            lassoLoopController.Upgrade1();
    }
}
