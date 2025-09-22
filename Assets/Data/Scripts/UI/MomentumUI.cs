using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MomentumUI : MonoBehaviour {

    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI _momentumValueOverlay;
    [SerializeField] private GameObject _buffOverlay;
    [SerializeField] private Slider _speedStatBar;

    private Animator _momentumBarAnimator;

    private static MomentumUI momentumUI;

    private void Start() {
        momentumUI = this; // temp // until I decided how I want to handle UI handlers
        _momentumBarAnimator = _speedStatBar.GetComponent<Animator>();
    }
    
    public void UpdateSpeedMult(float value) {
        if (_momentumValueOverlay != null) _momentumValueOverlay.text = "Momentum: " + value;
        else Debug.LogWarning(this + " Speed Mult Display is null");
    }

    public void UpdateSpeedBar(float value) {
        if (_speedStatBar != null) _speedStatBar.value = Mathf.Lerp(_speedStatBar.value, value, Time.fixedDeltaTime * 4);
        else Debug.LogWarning(this + " Momentum Bar is null");
    }

    public void ToggleBuffOverlay(bool toggle) {
        _buffOverlay.SetActive(toggle);
    }

    public void ActionFailed() {
        _momentumBarAnimator.SetTrigger("Flash");
    }
    public static MomentumUI GetSelf() {
        return momentumUI;
    }
}