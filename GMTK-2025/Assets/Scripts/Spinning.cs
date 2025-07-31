using UnityEngine;

public class Spinning : MonoBehaviour
{

    [SerializeField] private float spinSpeed;
    [SerializeField] private Vector3 spinAxis;

    private Quaternion initRotation;
    private float spinCounter = 0;

    void Start()
    {
        initRotation = transform.localRotation;
    }

    void Update()
    {
        transform.localRotation = initRotation * Quaternion.Euler(spinAxis * spinCounter);
        spinCounter += spinSpeed * Time.deltaTime;
        spinCounter %= 360;
    }
    
}
