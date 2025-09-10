using UnityEngine;

public class SpeedBuff : MonoBehaviour {

    private bool entered = false;

    // How far up and down the orb floats
    private float amplitude = 0.1f;
    // How fast the orb floats up and down
    private float frequency = 0.4f;
    private Vector3 startPos;

    private void Start() { startPos = transform.position; }

    private void FixedUpdate() {
        // Hover effect
        float offsetY = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
        transform.position = startPos + Vector3.up * offsetY;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !entered) {
            entered = true;
            Player player = other.GetComponentInParent<Player>();
            player.ChangeSpeedStat(50f);
            player.Buff(5);
            gameObject.SetActive(false);
        }
    }
}
