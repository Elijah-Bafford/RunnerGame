using TMPro;
using UnityEngine;

public class LevelComplete : MonoBehaviour {

    [SerializeField] GameStateHandler gameStateHandler;
    [SerializeField] GameTimer timer;
    [SerializeField] Player player;
    [SerializeField] TextMeshProUGUI timeNum;
    [SerializeField] TextMeshProUGUI speedNum;

    private MomentumMechanic momentumMech;

    private void Start() {
        momentumMech = player.GetComponent<MomentumMechanic>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            OnLevelComplete();
        }
    }

    private void OnLevelComplete() {
        gameStateHandler.TriggerLevelComplete();
        speedNum.text = momentumMech.GetHighestSpeed().ToString("F3");
        timeNum.text = timer.GetTimeAsString();
        RecordHandler.Instance.CreateRecord(SceneHandler.currentLevel, timer.GetTime(), momentumMech.GetHighestSpeed());
        RecordHandler.Instance.SaveRecords();
    }

}
