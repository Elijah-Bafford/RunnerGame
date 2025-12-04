using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/Max Momentum Up")]
public class MomentumCapUp : PowerUpEffect {
    [Tooltip("The value to set/change the momentum cap by")]
    [SerializeField] private float value = 1.0f;
    [Tooltip("False: Add 'value' to momentum cap. True: Set the momentum cap")]
    [SerializeField] private bool isSet = false;
    public override void Apply(Player player) {
        player.ChangeMomentumCap(value, !isSet);
    }
}
