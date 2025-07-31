using UnityEngine;

public class MakeChildrenRigidbodies : MonoBehaviour
{

    public void MakeRigidbodies()
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = child.gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false; // Ensure the Rigidbody is not kinematic
            rb.useGravity = true; // Enable gravity if needed
        }
    }
}
