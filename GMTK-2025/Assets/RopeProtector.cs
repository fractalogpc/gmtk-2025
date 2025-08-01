using UnityEngine;

public class RopeProtector : MonoBehaviour
{
    public Rigidbody[] joints;

    public Rigidbody jointStart;
    public Rigidbody jointEnd;

    public float maxForce = 100f;

    void Update()
    {
        // If any joint is moving too fast, reset the rope
        foreach (var joint in joints)
        {
            if (joint.linearVelocity.magnitude > maxForce)
            {
                // Debug.Log("Resetting rope due to excessive joint velocity.");
                ResetRope();
                return;
            }
        }
    }

    private void ResetRope()
    {
        // Reset the rope by reinitializing the joints and positions
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].position = jointStart.position + (jointEnd.position - jointStart.position) * (i / (float)(joints.Length - 1));
            joints[i].linearVelocity = Vector3.zero;
            joints[i].angularVelocity = Vector3.zero;
        }
    }

}
