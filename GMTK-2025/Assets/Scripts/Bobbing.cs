using UnityEngine;

public class Bobbing : MonoBehaviour
{

    [SerializeField] private float _maxHeight = 5f;
    [SerializeField] private float _timePerCycle = 2f;
    [SerializeField] private Vector3 _axis = Vector3.up;
    [SerializeField] private AnimationCurve _bobbingCurve;
    [SerializeField] private float _timeOffset = 0f;
    [SerializeField] private float _maxRotation = 15f;

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

        // Noise rotation
        if (_maxRotation == 0) return;
        float rotation = Mathf.Sin((Time.time + _timeOffset) / _timePerCycle * Mathf.PI * 2) * _maxRotation;
        transform.localRotation = Quaternion.Euler(_axis * rotation + new Vector3(0, rotation, 0));
    }
    
}
