using TMPro;
using UnityEngine;

public class LevelComplete : MonoBehaviour {

    [SerializeField] GameStateHandler gameStateHandler;
    [SerializeField] GameTimer timer;
    [SerializeField] Player player;
    [SerializeField] TextMeshProUGUI timeNum;
    [SerializeField] TextMeshProUGUI speedNum;
    [Header("The \"record\" text in the Level Complete Overlay")]
    [SerializeField] GameObject ARecord;
    [Header("Colors")]
    [SerializeField] private Color gold;
    [SerializeField] private Color blue;
    [SerializeField] private Color red;

    private MomentumMechanic momentumMech;

    private void Start() {
        momentumMech = player.GetComponent<MomentumMechanic>();
        RecordHandler.OnRecordUpdated += HandleRecordUpdated;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            OnLevelComplete();
        }
    }

    private void OnLevelComplete() {
        gameStateHandler.TriggerLevelComplete();
        speedNum.text = momentumMech.GetHighestSpeed().ToString();
        timeNum.text = GameTimer.GetTimeAsString(true);
        RecordHandler.Instance.UpdateRecord(SceneHandler.currentLevel, timer.GetCurrentTime(), momentumMech.GetHighestSpeed());
    }

    private void HandleRecordUpdated(int level, LevelRecord record, bool isTimeRecord, bool isMomentumRecord) {
        if (ARecord != null) ARecord.SetActive(isTimeRecord || isMomentumRecord);
        speedNum.color = isMomentumRecord ? gold : blue;
        timeNum.color = isTimeRecord ? gold : red;
    }
}
