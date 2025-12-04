using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/Start Focus Up")]
public class StartFocusUp : PowerUpEffect {

    [Tooltip("How much to increase the player's starting focus")]
    [SerializeField] private float value = 20f;
    [Tooltip("False: Add 'value' to starting focus. True: Set starting focus to value.")]
    [SerializeField] private bool isSet = false;
    public override void Apply(Player player) {
        player.ChangeStartFocus(value, addToCurrent: !isSet);
    }
}

