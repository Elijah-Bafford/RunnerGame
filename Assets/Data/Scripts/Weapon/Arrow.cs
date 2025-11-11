using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour {

    [SerializeField, Range(1f, 100f)] private float speed = 10f;
    [SerializeField, Range(1f, 25f)] private float lifetime = 10f;
    [SerializeField] private Rigidbody rb;
    private Player player;

    private void Awake() => player = Player.Instance;

    private void Start() {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter(Collider other) {
        //Debug.Log("Arrow hit: " + other.name + " (tag " + other.tag + ")");

        if (other.name == "AttackBox") {
            rb.linearVelocity = speed * player.transform.forward;
            rb.useGravity = true;
            Destroy(gameObject, 1f);
            return;
        }

        switch (other.tag) {
            case "Wall":
            case "Ground":
            case "SlopedGround":
                rb.linearVelocity = Vector3.zero;
                break;
            case "Player":
                player.SetDead(true);
                Destroy(gameObject); break;
            default: return;
        }

    }
}
