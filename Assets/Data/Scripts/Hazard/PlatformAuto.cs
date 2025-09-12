using UnityEngine;

public class PlatformAuto : MonoBehaviour {
    [Header("Automation")]
    [Tooltip("The location that the platform moves to")]
    [SerializeField] private Vector3 _targetPosition;
    [Tooltip("Use the relative position of the platform's start position")]
    [SerializeField] private bool _useRelativePosition = true;
    [Tooltip("The amount of time it takes (in seconds) for the platform to arrive at the target position.")]
    [SerializeField] private float _timeToArrive = 1f;

    public Vector3 CurrentVelocity { get; private set; }

    private Vector3 _lastPosition;
    private Vector3 _startPosition;
    private float _frequency = 1f;

    private void Start() {
        _startPosition = transform.position;
        if (_useRelativePosition) _targetPosition += _startPosition;
        if (_timeToArrive <= 0) _timeToArrive = 1;
        
        _frequency = 1f / _timeToArrive;

        _lastPosition = transform.position;
    }

    private void FixedUpdate() {
        // Move the platform
        float phase = Time.time * _frequency * Mathf.PI * 2f;
        float sinValue = Mathf.Sin(phase);
        float t = (sinValue + 1f) * 0.5f;
        Vector3 newPos = Vector3.Lerp(_startPosition, _targetPosition, t);
        transform.position = newPos;

        // After moving
        CurrentVelocity = (newPos - _lastPosition) / Time.fixedDeltaTime;
        _lastPosition = newPos;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Vector3 center = _useRelativePosition ? transform.position + _targetPosition : _targetPosition;
        Gizmos.DrawSphere(center, 1f);
    }
}