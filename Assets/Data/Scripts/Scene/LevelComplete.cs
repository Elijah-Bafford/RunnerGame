using TMPro;
using UnityEngine;

public class LevelComplete : MonoBehaviour {

    [Header("Game Input Object")]
    [SerializeField] private GameStateHandler gameStateHandler;
    [Header("Game Timer Object")]
    [SerializeField] private GameTimer timer;
    [Header("Level Complete Overlay TMPs")]
    [SerializeField] private TextMeshProUGUI timeNum;
    [SerializeField] private TextMeshProUGUI momentumNum;
    [Header("The \"record\" text in the Level Complete Overlay")]
    [SerializeField] private GameObject ARecord;
    [Header("Colors")]
    [SerializeField] private Color gold;
    [SerializeField] private Color blue;
    [SerializeField] private Color red;

    private MomentumMechanic momentumMech;

    private void Start() {
        momentumMech = Player.Instance.GetComponent<MomentumMechanic>();
        RecordHandler.OnRecordUpdated += HandleRecordUpdated;
    }

    private void OnTriggerEnter(Collider other) {
        print(other.name);
        //OnLevelComplete();
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
