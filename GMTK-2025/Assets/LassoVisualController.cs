using UnityEngine;

public class LassoVisualController : MonoBehaviour
{
    public GameObject ropeStart;
    public GameObject ropeEnd;

    public GameObject ropePrefab;
    private GameObject rope;

    public Transform lassoOrigin;
    public Transform lassoGoal;

    public LassoLoopController lassoLoopController;

    private void Start()
    {
        lassoLoopController = GetComponentInChildren<LassoLoopController>();
    }

    bool isEnabled;

    void Update()
    {
        if (!isEnabled) return;
        ropeStart.transform.position = lassoOrigin.position + Vector3.down * 1.0f;
        ropeEnd.transform.position = lassoLoopController.ropeEnd.position;
    }

    public void EnableVisual()
    {
        // Debug.Log("Resetting");
        rope = Instantiate(ropePrefab, transform.position, transform.rotation);

        ropeStart = rope.transform.GetChild(1).gameObject;
        ropeEnd = rope.transform.GetChild(10).gameObject;

        lassoLoopController = rope.GetComponentInChildren<LassoLoopController>();
        lassoLoopController.center = lassoGoal;
        lassoLoopController.CreatePrefabs();

        isEnabled = true;
    }

    public void HitGround()
    {
        Debug.Log("Hit gorund");
        lassoLoopController.onGround = true;
    }

    public void DisableVisual()
    {
        isEnabled = false;

        Destroy(rope);
    }
}
