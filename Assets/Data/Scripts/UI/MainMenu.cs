using UnityEngine;

public class MainMenu : MonoBehaviour {

    [Header("Scene Handler Ref")]
    [SerializeField] private SceneHandler sceneHandler;

    [Header("Level Buttons")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private RectTransform scrollContent;

    [Header("Menus")]
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject levelSelectMenu;



    private void Start() {
        ShowLevelSelectMenu(false);
        for (int i = 0; i < SceneHandler.numLevels; i++) {
            if (i == 0) continue;

            GameObject btnObj = Instantiate(levelButtonPrefab, scrollContent);
            LevelButtonUI buttonUI = btnObj.GetComponent<LevelButtonUI>();

            buttonUI.levelText.text = $"Level {i}";

            LevelRecord record = RecordHandler.Instance.GetRecord(i);

            buttonUI.momentumNum.text = record.highestMomentum == 0 ? "None" : record.highestMomentum.ToString();
            buttonUI.timeNum.text = record.fastestTime == 0 ? "None" : GameTimer.GetTimeAsString(false, record.fastestTime);

            int levelIndex = i;
            if (!record.unlocked) {
                buttonUI.button.interactable = false;
            } else {
                buttonUI.button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
            }
        }
    }

    private void OnLevelButtonClicked(int levelIndex) {
        sceneHandler.LoadLevel(levelIndex);
    }

    private void ShowLevelSelectMenu(bool show) {
        levelSelectMenu.SetActive(show);
        titleMenu.SetActive(!show);
    }

    // Button events
    public void StartGame() { ShowLevelSelectMenu(true); }
    public void Return_LSM() { ShowLevelSelectMenu(false); }
    public void QuitGame() { Application.Quit(); }

    public void QuitInEditor() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}