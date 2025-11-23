using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/Speed Buff")]
public class SpeedBuff : PowerUpEffect {
    [Tooltip("How much to multiply the player's momentum gain by")]
    [SerializeField] private float SpeedBuffMultiplier = 2f;
    [Tooltip("How long the buff lasts")]
    [SerializeField] private float SpeedBuffTime = 5f;
    [Tooltip("How much to increase the player's SpeedStat")]
    [SerializeField] private float SpeedStatBoost = 50f;
    public override void Apply(Player player) =>
        player.SpeedBuff(SpeedBuffTime, SpeedBuffMultiplier, SpeedStatBoost);
}
