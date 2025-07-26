using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {

    [Header("Animation Event Handler Scripts")]
    [Tooltip("Weapon Script")]
    [SerializeField] private PlayerAttack attack;
    [Tooltip("Player Script")]
    [SerializeField] Player player;
    [Tooltip("UI Input Script")]
    [SerializeField] private GameStateHandler uiInput;

    public void DisableWeapon() { attack.HasAttacked(false); }
    public void AllowAttack() { player.ResetIsInAttack(); }
    public void PlayerDied() { player.Died(); }
    public void StopCombat() { uiInput.SetGameState(GameStateHandler.GameState.Death); }

}
