using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour {

    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;
    private Rigidbody rb;
    public void DestroyArrow(float lifetime = 0) => Destroy(gameObject, lifetime);

    private void Awake() => rb = GetComponent<Rigidbody>();
    
    private void Start() {
        rb.linearVelocity = transform.forward * speed;
        DestroyArrow(lifetime);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Arrow hit: " + other.name + " (tag " + other.tag + ")");

        switch (other.tag) {
            case "Wall":
            case "Ground":
            case "SlopedGround":
                break;
            case "Player":
                // Logic for player hit by arrow
                break;
            default: return;
        }
        DestroyArrow();
    }
}
