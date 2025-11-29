using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/Max Focus Up")]
public class MaxFocusUp : PowerUpEffect {

    [Tooltip("How much to increase the player's max focus")]
    [SerializeField] private float increaseValue = 20f;
    public override void Apply(Player player) =>
        player.ChangeMaxFocus(increaseValue);
}
