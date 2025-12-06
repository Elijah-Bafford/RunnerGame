using UnityEngine;
using UnityEngine.InputSystem;

public class PlayingInput : MonoBehaviour {

    private Player player;

    private void Awake() => player = Player.Instance;

    public void OnMove(InputAction.CallbackContext context) {
        player.Perform(Player.Act.Move, context.ReadValue<Vector2>().normalized); 
    }
    public void OnJump(InputAction.CallbackContext context) {
        if (context.performed) player.Perform(Player.Act.Jump, default, false);
        else if (context.canceled) player.Perform(Player.Act.Jump, default, true);
    }
    public void OnAttack(InputAction.CallbackContext context) {
        if (context.performed) player.Perform(Player.Act.Attack);
        
    }
    public void OnSlide(InputAction.CallbackContext context) { 
        if (context.performed) player.Perform(Player.Act.Slide, default, false);
        else if (context.canceled) player.Perform(Player.Act.Slide, default, true);
    }

    public void OnGrapple(InputAction.CallbackContext context) { if (context.performed) player.Perform(Player.Act.Grapple); }

    public void Temp(InputAction.CallbackContext context) {
        if (context.performed) {
            player.ChangeMaxFocus(20f);
        }
    }
}
