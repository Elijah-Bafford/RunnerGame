using System;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    //private enum PowerUpType { MaxSpeedStatUp, StartSpeedStatUp }

    [Tooltip("Unique item ID for single time pick up. Leave as 0 to always respawn.")]
    [SerializeField] private int ItemID = 0;
    [SerializeField] private bool UseFloating = true;
    [SerializeField] private PowerUpEffect powerUpEffect;
    [SerializeField] private PowerUpEffect powerUpEffectAlt;

    private NotificationNode node;
    private bool entered = false;

    // How far up and down the orb floats
    private float amplitude = 0.1f;
    // How fast the orb floats up and down
    private float frequency = 0.4f;
    private Vector3 startPos;

    private void Start() {
        node = GetComponent<NotificationNode>();
        Init();
        startPos = transform.position;
        GameStateHandler.OnLevelRestart += OnLevelRestart;

    }

    private void OnDestroy() => GameStateHandler.OnLevelRestart -= OnLevelRestart;

    private void Init() {
        Debug.Log(powerUpEffect.info + " Collected = " + IsCollected());
        if (!IsCollected()) return;
        if (powerUpEffectAlt == null) {
            gameObject.SetActive(false);
            return;
        }
        Debug.Log("Enabling alt. effect");
        powerUpEffect = powerUpEffectAlt;
        entered = false;
        if (node != null) node.Disable();

        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    private void OnLevelRestart() {
        if (entered) Init(); // only reset if this item was picked up during the run
    }

    private bool IsCollected() => ItemID != 0 && PlayerData.Data.IsCollected(ItemID);

    private void FixedUpdate() {
        if (!UseFloating) return;
        // Hover effect
        float offsetY = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
        transform.position = startPos + Vector3.up * offsetY;
    }

    private void OnTriggerEnter(Collider other) {
        if (entered) return;
        if (!other.CompareTag("Player")) return;
        if (ItemID != 0) PlayerData.Data.CollectItem(ItemID);
        entered = true;
        powerUpEffect.Apply(Player.Instance);
        gameObject.SetActive(false);
    }
}

public abstract class PowerUpEffect : ScriptableObject {
    public string info;
    public abstract void Apply(Player player);

}