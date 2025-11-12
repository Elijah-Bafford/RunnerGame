using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour {

    private Player player;

    private void Awake() => player = Player.Instance;

    public void OnAttackStart() => player.isInAttack = true;
    public void OnAttackEnd() => player.isInAttack = false;
    public void EnableAttackBox() => player.SetAttackBoxEnabled(true);
    public void DisableAttackBox() => player.SetAttackBoxEnabled(false);
}
