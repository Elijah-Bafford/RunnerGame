using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {

    [Header("Animation Event Handler Scripts")]
    [Tooltip("Weapon Script")]
    [SerializeField] private PlayerAttack attack;
    [Tooltip("Player Script")]
    [SerializeField] Player player;
    [Tooltip("Player Script")]
    [SerializeField] private UIInput uiInput;

    public void EnableWeapon() { attack.EnableWeapon(); }
    public void DisableWeapon() { attack.DisableWeapon(); }
    public void AllowAttack() { player.IsInAttack(); }
    public void PlayerDied() { player.Died(); }
    public void StopCombat() { uiInput.SetGameState(UIInput.GameState.Death); }

}
