using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Arrow : MonoBehaviour {

    private float velocity = 5f;
    private float distance = 15f;
    private Vector3 targetPosition;
    public void DestroyArrow() => Destroy(gameObject);

    private void Awake() {
        targetPosition = transform.forward * distance + transform.position;
    }

    private void FixedUpdate() {

        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime / velocity);
        transform.position = newPos;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            print("Hit Player");
        }
        if (other.name != "Player Detection") {
            DestroyArrow();
        }
    }
}
