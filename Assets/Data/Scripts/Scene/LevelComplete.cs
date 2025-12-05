using System;
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
    [SerializeField] private TextMeshProUGUI itemsCollected;
    [Header("The \"record\" text in the Level Complete Overlay")]
    [SerializeField] private GameObject ARecord;
    [Header("Colors")]
    [SerializeField] private Color gold;
    [SerializeField] private Color purple;
    [SerializeField] private Color red;

    private MomentumMechanic momentumMech;

    private void Start() {
        momentumMech = Player.Instance.GetComponent<MomentumMechanic>();
        RecordHandler.OnRecordUpdated += HandleRecordUpdated;
    }

    private void OnDestroy() => RecordHandler.OnRecordUpdated -= HandleRecordUpdated;


    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) OnLevelComplete();
    }

    private void OnLevelComplete() {
        gameStateHandler.TriggerLevelComplete();
        momentumNum.text = momentumMech.HighestMomentumToString();
        timeNum.text = GameTimer.GetTimeAsString(true);
        itemsCollected.text = RecordHandler.Instance.ItemsCollectedToString(SceneHandler.CurrentLevel);
        RecordHandler.Instance.UpdateRecord(SceneHandler.CurrentLevel, timer.GetCurrentTime(), momentumMech.GetHighestMomentum());
        AudioHandler.Instance.StopAll();
    }

    private void HandleRecordUpdated(int level, LevelRecord record, bool isTimeRecord, bool isMomentumRecord) {
        if (ARecord != null) ARecord.SetActive(isTimeRecord || isMomentumRecord);
        momentumNum.color = isMomentumRecord ? gold : purple;
        timeNum.color = isTimeRecord ? gold : red;
    }
}
