using TMPro;
using UnityEngine;

public class LevelComplete : MonoBehaviour {


    [SerializeField] Player player;
    [Header("Game Input Object")]
    [SerializeField] GameStateHandler gameStateHandler;
    [Header("Game Timer Object")]
    [SerializeField] GameTimer timer;
    [Header("Level Complete Overlay TMPs")]
    [SerializeField] TextMeshProUGUI timeNum;
    [SerializeField] TextMeshProUGUI momentumNum;
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
        momentumNum.text = momentumMech.GetHighestSpeed().ToString();
        timeNum.text = GameTimer.GetTimeAsString(true);
        RecordHandler.Instance.UpdateRecord(SceneHandler.currentLevel, timer.GetCurrentTime(), momentumMech.GetHighestSpeed());
    }

    private void HandleRecordUpdated(int level, LevelRecord record, bool isTimeRecord, bool isMomentumRecord) {
        if (ARecord != null) ARecord.SetActive(isTimeRecord || isMomentumRecord);
        momentumNum.color = isMomentumRecord ? gold : blue;
        timeNum.color = isTimeRecord ? gold : red;
    }
}
