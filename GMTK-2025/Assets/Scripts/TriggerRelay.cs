using UnityEngine;
using UnityEngine.Events;

public class TriggerRelay : MonoBehaviour
{

    public string targetTag = "LassoTarget";

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            onTriggerExit?.Invoke();
        }
    }
}
