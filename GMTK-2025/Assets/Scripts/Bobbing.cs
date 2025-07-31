using UnityEngine;

public class Bobbing : MonoBehaviour
{

    [SerializeField] private float _maxHeight = 5f;
    [SerializeField] private float _timePerCycle = 2f;
    [SerializeField] private Vector3 _axis = Vector3.up;
    [SerializeField] private AnimationCurve _bobbingCurve;
    [SerializeField] private float _timeOffset = 0f;

    private Vector3 _initialPosition;

    private void Start()
    {
        _initialPosition = transform.localPosition;
    }

    private void Update()
    {
        float t = Mathf.PingPong((Time.time + _timeOffset) / _timePerCycle, 1);
        float height = (_bobbingCurve.Evaluate(t) - 0.5f) * _maxHeight;
        transform.localPosition = _initialPosition + _axis * height;
    }
    
}
