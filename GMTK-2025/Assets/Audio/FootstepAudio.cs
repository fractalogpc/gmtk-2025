using FMODUnity;
using KinematicCharacterController;
using Player;
using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private float _footstepDistance = 1.8f;
    [SerializeField] private KinematicCharacterMotor _kinematicCharacterMotor;
    public StudioEventEmitter _footstepEmitter;
    private Vector3 _lastFootstepPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _lastFootstepPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (_playerController.MoveInputVector.magnitude < 0.1f) return;
        if (!_kinematicCharacterMotor.GroundingStatus.IsStableOnGround) return;
        Vector3 positionXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 lastPositionXZ = new Vector3(_lastFootstepPosition.x, 0, _lastFootstepPosition.z);
        if (!(Vector3.Distance(positionXZ, lastPositionXZ) > _footstepDistance)) return;
        _footstepEmitter.Play();
        _lastFootstepPosition = transform.position;
    }
}
