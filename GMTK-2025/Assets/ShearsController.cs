using UnityEngine;

public class ShearsController : MonoBehaviour
{
    public Transform shearsOrigin;

    public void LateUpdate()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, shearsOrigin.position, 0.6f);
        transform.SetPositionAndRotation(targetPosition, shearsOrigin.rotation);
    }
}
