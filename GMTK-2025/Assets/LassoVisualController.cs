using UnityEngine;

public class LassoVisualController : MonoBehaviour
{
    public GameObject ropeStart;
    public GameObject ropeEnd;

    public GameObject ropePrefab;
    private GameObject rope;

    public Transform lassoOrigin;
    public Transform lassoGoal;

    bool isEnabled;

    void Update()
    {
        if (!isEnabled) return;
        ropeStart.transform.position = lassoOrigin.position + Vector3.down * 1.0f;
        ropeEnd.transform.position = lassoGoal.position;
    }

    private void OnEnable()
    {
        EnableVisual();
    }

    public void EnableVisual()
    {
        // Debug.Log("Resetting");
        rope = Instantiate(ropePrefab, transform.position, transform.rotation);

        ropeStart = rope.transform.GetChild(0).gameObject;
        ropeEnd = rope.transform.GetChild(9).gameObject;

        isEnabled = true;
    }

    public void DisableVisual()
    {
        isEnabled = false;

        Destroy(rope);
    }
}
