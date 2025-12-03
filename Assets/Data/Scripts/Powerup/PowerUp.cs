using UnityEngine;

public class PowerUp : MonoBehaviour {

    //private enum PowerUpType { MaxSpeedStatUp, StartSpeedStatUp }

    [SerializeField] private int ItemID = 0;
    [SerializeField] private bool UseFloating = true;
    [SerializeField] private PowerUpEffect powerUpEffect;


    private bool entered = false;

    // How far up and down the orb floats
    private float amplitude = 0.1f;
    // How fast the orb floats up and down
    private float frequency = 0.4f;
    private Vector3 startPos;

    private void Start() {
        if (PlayerData.Data.IsCollected(ItemID)) gameObject.SetActive(false);

        startPos = transform.position;
    }

    private void FixedUpdate() {
        if (!UseFloating) return;
        // Hover effect
        float offsetY = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
        transform.position = startPos + Vector3.up * offsetY;
    }

    private void OnTriggerEnter(Collider other) {
        if (entered) return;
        if (!other.CompareTag("Player")) return;
        print("Player entered!");
        PlayerData.Data.CollectItem(ItemID);
        entered = true;
        powerUpEffect.Apply(Player.Instance);
        Destroy(gameObject);
    }
}

public abstract class PowerUpEffect : ScriptableObject {
    public string info;
    public abstract void Apply(Player player);

}