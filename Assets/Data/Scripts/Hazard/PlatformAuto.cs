using UnityEngine;

public class PlatformAuto : MonoBehaviour {
    [Header("Automation")]
    [Tooltip("The location that the platform moves to")]
    [SerializeField] Vector3 _targetPosition;
    [Tooltip("Use the relative position of the platform's start position")]
    [SerializeField] bool _useRelativePosition = false;
    [Tooltip("Oscillation frequency in cycles per second")]
    [SerializeField] float _frequency = 0.5f; // 0.5 Hz = back/forth every 2s

    private Vector3 _startPosition;

    private void Start() {
        _startPosition = transform.position;
        if (_useRelativePosition) {
            _targetPosition += _startPosition;
        }
    }

    private void FixedUpdate() {
        MovePlatform();
    }

    private void MovePlatform() {
        float phase = Time.time * _frequency * Mathf.PI * 2f;
        float sinValue = Mathf.Sin(phase);
        float t = (sinValue + 1f) * 0.5f;
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Vector3 center = _targetPosition;
        if (_useRelativePosition) {
            center = _targetPosition + transform.position;
        } else {
            
        }
        Gizmos.DrawSphere(center, 1f);
    }

}
