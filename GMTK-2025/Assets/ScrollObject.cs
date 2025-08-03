using UnityEngine;

public class ScrollObject : MonoBehaviour
{

    [SerializeField] private float scrollSpeed = 1f;

    void Update()
    {
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
    }
    
}
