using UnityEngine;

public class RopeController : MonoBehaviour
{
    public ConfigurableJoint[] joints;

    public void SetJointLengths(float length)
    {
        foreach (var joint in joints)
        {
            if (joint != null)
            {
                // Set the linear limit of the joint to the specified length
                SoftJointLimit limit = joint.linearLimit;
                limit.limit = length;
                joint.linearLimit = limit;
            }
        }
    }
}
