using KinematicCharacterController;
using Player;
using UnityEngine;

public class PlayerViewBob : MonoBehaviour
{

  [SerializeField] private PlayerController _playerController;
  [SerializeField] private GameObject _footprintPrefab;
  [SerializeField] private float _footprintStrideWidth = 0.5f;
  [SerializeField] private Transform _footprintContainer;
  [SerializeField] private AnimationCurve _viewBobVerticalCurve;
  [SerializeField] private float _verticalMultiplier;
  [SerializeField] private AnimationCurve _viewBobHorizontalCurve;
  [SerializeField] private float _horizontalMultiplier;
  [SerializeField] private AnimationCurve _viewBobVelocityCurve;
  [SerializeField] private AnimationCurve _viewBobVelocityAmplitudeCurve;
  [Range(0.1f, 10f)] [SerializeField] private float _velocityRange = 1f;
  [SerializeField] private Vector2 _breathingFrequencyRange = new Vector2(0.5f, 1.5f);
  [SerializeField] private Vector2 _breathingAmplitudeRange = new Vector2(0.1f, 0.3f);
  [SerializeField] private AnimationCurve _breathingVelocityCurve;
  [SerializeField] private AnimationCurve _breathingCurve;
  [SerializeField] private float _viewBobFrequency = 1f;

  [Header("Noise")]
  [SerializeField] private float _noiseFrequency = 1f;
  [SerializeField] private float _noiseAmplitude = 1f;
  [SerializeField] private float _breathingNoiseFrequency = 1f;
  [SerializeField] private float _breathingNoiseAmplitude = 1f;


  private KinematicCharacterMotor _motor;
  private Vector3 _baseVelocity;
  private float _timeSinceLastStep;
  private bool _stepRight;
  private float _currentCurvePosition = 1;
  private float _currentBreathingPosition = 1;
  private Vector3 _cameraOffset;
  public Vector3 CameraOffset => _cameraOffset;

  private void Start() {
    _motor = _playerController.Motor;
  }

  private void Update() {
    _baseVelocity = _motor.BaseVelocity;
    Vector2 velocity = new Vector2(_baseVelocity.x, _baseVelocity.z);
    float frequency = _viewBobFrequency * _viewBobVelocityCurve.Evaluate(velocity.magnitude / _velocityRange);

    _currentCurvePosition += Time.deltaTime * frequency;
    _timeSinceLastStep += Time.deltaTime;

    ProcessStep();

    if (_motor.GroundingStatus.IsStableOnGround) {
      if (_baseVelocity.magnitude > 0.1f) {
        if (_timeSinceLastStep > 1f / frequency && _currentCurvePosition >= 1) {
          _timeSinceLastStep = 0;
          _currentCurvePosition = 0;
          _stepRight = !_stepRight;

          if (_footprintPrefab != null) {
            Quaternion rotation = Quaternion.LookRotation(_motor.CharacterForward, Vector3.up);
            Vector3 position = _playerController.transform.position + (_stepRight ? _motor.CharacterRight : -_motor.CharacterRight) * _footprintStrideWidth;
            GameObject footprint = Instantiate(_footprintPrefab, position, rotation, _footprintContainer);
            Destroy(footprint, 10f);
          }
        }
      }
    }

  }

  private void ProcessStep()
  {
    Vector2 velocity = new Vector2(_baseVelocity.x, _baseVelocity.z);
    float multiplier = _viewBobVelocityAmplitudeCurve.Evaluate(velocity.magnitude / _velocityRange);
    float verticalOffset = _viewBobVerticalCurve.Evaluate(_currentCurvePosition) * _verticalMultiplier * multiplier;
    float horizontalOffset = _viewBobHorizontalCurve.Evaluate(_currentCurvePosition) * _horizontalMultiplier * multiplier;

    if (!_stepRight)
    {
      horizontalOffset = -horizontalOffset;
    }

    float magnitude = _viewBobVelocityCurve.Evaluate(velocity.magnitude / _velocityRange);

    float noise = (Mathf.PerlinNoise(Time.time * _noiseFrequency, 0) - 0.5f) * _noiseAmplitude * magnitude;
    horizontalOffset += noise;
    noise = Mathf.PerlinNoise(0, Time.time * _noiseFrequency) * _noiseAmplitude * magnitude;
    verticalOffset += noise;

    // Breathing effect
    float curveProgress = velocity.magnitude / _velocityRange;
    float breathingFrequency = Mathf.Lerp(_breathingFrequencyRange.x, _breathingFrequencyRange.y, _breathingVelocityCurve.Evaluate(curveProgress));
    float breathingAmplitude = Mathf.Lerp(_breathingAmplitudeRange.x, _breathingAmplitudeRange.y, _breathingVelocityCurve.Evaluate(curveProgress));

    _currentBreathingPosition += Time.deltaTime * breathingFrequency;
    _currentBreathingPosition %= 1;

    float breathingOffset = _breathingCurve.Evaluate(_currentBreathingPosition) * breathingAmplitude;
    Vector2 breathingNoise = new Vector2(
      (Mathf.PerlinNoise(Time.time * _breathingNoiseFrequency, 0) - 0.5f) * _breathingNoiseAmplitude * breathingAmplitude,
      (Mathf.PerlinNoise(0, Time.time * _breathingNoiseFrequency) - 0.5f) * _breathingNoiseAmplitude * breathingAmplitude
    );

    horizontalOffset += breathingNoise.x;
    verticalOffset += breathingNoise.y;
    verticalOffset += breathingOffset;

    _cameraOffset = new Vector3(horizontalOffset, verticalOffset, 0);
    
    transform.localPosition = _cameraOffset;
  }

}
