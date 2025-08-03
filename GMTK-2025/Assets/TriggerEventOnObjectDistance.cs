using UnityEngine;
using UnityEngine.Events;

public class TriggerEventOnObjectDistance : MonoBehaviour
{
    public Transform targetObject;
    public float triggerDistance = 5f;
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    private void Update()
    {
        if (targetObject == null) return;

        float distance = Vector3.Distance(transform.position, targetObject.position);
        if (distance < triggerDistance)
        {
            onTriggerEnter.Invoke();
        }
        else
        {
            onTriggerExit.Invoke();
        }
    }
}
