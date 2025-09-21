using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {

    private PlayerAttack playerAttack;
    private Player player;
    [Header("Animation Event Handler Scripts")]
    [Tooltip("UI Input Script")]
    [SerializeField] private GameStateHandler uiInput;

    public void InitAnimationEventHandler(Player player, PlayerAttack playerAttack) {
        this.player = player;
        this.playerAttack = playerAttack;
    }

    public void DisableWeapon() { playerAttack.HasAttacked(false); }
    public void AllowAttack() { player.ResetIsInAttack(); }
    public void StopCombat() { uiInput.SetGameState(GameStateHandler.GameState.Death); }

}
