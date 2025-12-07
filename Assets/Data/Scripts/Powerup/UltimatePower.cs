using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class UltimatePower : MonoBehaviour {


    private float lastMaxFocus = 20f;

    private Player player;

    private void Start() {
        player = Player.Instance;
        GameStateHandler.OnLevelRestart += OnLevelRestart;
    }

    private void OnDestroy() {
        GameStateHandler.OnLevelRestart -= OnLevelRestart;
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        lastMaxFocus = player.MaxFocus;
        player.ChangeMaxFocus(999, false, true, false);
        StatusUI.Instance.UpdateFocusBar(999, instant: true);
        gameObject.SetActive(false);
    }

    private void OnLevelRestart() {
        //if (gameObject.activeSelf) return;
        //gameObject.SetActive(true);
    }
}
