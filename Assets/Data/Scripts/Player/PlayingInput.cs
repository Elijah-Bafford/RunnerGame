using UnityEngine;
using UnityEngine.InputSystem;

public class PlayingInput : MonoBehaviour {

    [SerializeField] private Player player;
    private bool toggle = false;

    public void OnMove(InputAction.CallbackContext context) { player.Perform(Player.Act.Move, context.ReadValue<Vector2>().normalized); }
    public void OnJump(InputAction.CallbackContext context) {
        if (context.performed) player.Perform(Player.Act.Jump, default, false);
        else if (context.canceled) player.Perform(Player.Act.Jump, default, true);
    }
    public void OnAttack(InputAction.CallbackContext context) { if (context.performed) player.Perform(Player.Act.Attack); }
    public void OnSlide(InputAction.CallbackContext context) { if (context.performed || context.canceled) player.Perform(Player.Act.Slide); }

    public void OnGrapple(InputAction.CallbackContext context) { if (context.performed) player.Perform(Player.Act.Grapple); }

    public void DELETE(InputAction.CallbackContext context) {
        if (context.performed) {
            toggle = !toggle;
            Time.timeScale = toggle ? 1 : 0;
        }
    }
}
