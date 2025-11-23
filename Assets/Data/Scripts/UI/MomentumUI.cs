using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class MomentumUI : MonoBehaviour {

    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI _momentumValueOverlay;
    [SerializeField] private GameObject _buffOverlay;
    [SerializeField] private Slider _speedStatBar;
    [SerializeField] private TextMeshProUGUI _incDec;

    private TextMeshProUGUI _buffText;

    private Animator _momentumBarAnimator;
    public static MomentumUI Instance { get; private set; }

    private void Start() {
        Instance = this; // temp // until I decided how I want to handle UI handlers
        _momentumBarAnimator = _speedStatBar.GetComponent<Animator>();
        _buffText = _buffOverlay.GetComponent<TextMeshProUGUI>();
    }
    
    public void UpdateSpeedMult(float value) {
        if (_momentumValueOverlay != null) _momentumValueOverlay.text = "Momentum: " + value;
        else Debug.LogWarning(this + " Speed Mult Display is null");
    }

    public void UpdateIncDec(string s) {
        if (_incDec != null) _incDec.text = s;
        else Debug.LogWarning(this + " Inc/Dec Display is null");
    }

    public void UpdateSpeedBar(float value) {
        if (_speedStatBar != null) {
            _speedStatBar.value = Mathf.MoveTowards(_speedStatBar.value, value, Time.fixedDeltaTime * 25f);

        } else Debug.LogWarning(this + " Momentum Bar is null");
    }

    public void ToggleBuffOverlay(bool toggle, float multuplier) {
        _buffOverlay.SetActive(toggle);
        if (_buffText != null) _buffText.text = "(x" + multuplier + ")";
    }

    public void ActionFailed() => _momentumBarAnimator.SetTrigger("Flash");
    
}