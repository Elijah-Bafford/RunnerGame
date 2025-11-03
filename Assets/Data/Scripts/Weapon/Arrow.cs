using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour {

    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 10f;
    private Rigidbody rb;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Start() {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Arrow hit: " + other.name + " (tag " + other.tag + ")");

        switch (other.tag) {
            case "Wall":
            case "Ground":
            case "SlopedGround":
                rb.linearVelocity = Vector3.zero;
                break;
            case "Player":
                // Logic for player hit by arrow
                Destroy(gameObject); break;
            default: return;
        }

    }
}
