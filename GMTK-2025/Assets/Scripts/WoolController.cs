using UnityEngine;

public class WoolController : MonoBehaviour
{
    public Transform woolOrigin;


    public void LateUpdate()
    {
        Vector3 targetPosition = Vector3.Lerp(transform.position, woolOrigin.position, 0.4f);
        transform.SetPositionAndRotation(targetPosition, woolOrigin.rotation);
    }
}
