using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour {

    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI _momentumValueOverlay;
    [SerializeField] private TextMeshProUGUI _crosshair;
    [SerializeField] private TextMeshProUGUI _incNotification;
    [SerializeField] private TextMeshProUGUI _focusValue;
    [SerializeField] private Slider _focusBar;
    [SerializeField] private GameObject _grappleReticle;
    [SerializeField] private GameObject _momentumUp;

    private TextMeshProUGUI _buffText;

    private Animator _momentumBarAnimator;

    private float currentIncValue = 0;

    private byte crosshairState = 0;
    private float lastRawMomentum = 0f;
    private sbyte crosshairArrowState = 0;

    private UtilObjectActiveEvent incNotifyActivity;
    private UtilObjectActiveEvent gCrosshairNotifyActivity;

    public static StatusUI Instance { get; private set; }

    private void Start() {
        Instance = this; // temp // until I decided how I want to handle UI handlers
        _momentumBarAnimator = _focusBar.GetComponent<Animator>();
        _buffText = _momentumUp.GetComponent<TextMeshProUGUI>();
        incNotifyActivity = _incNotification.gameObject.GetComponent<UtilObjectActiveEvent>();
        gCrosshairNotifyActivity = _grappleReticle.gameObject.GetComponent<UtilObjectActiveEvent>();
        incNotifyActivity.OnDisabled += incNotifyDisabled;
        gCrosshairNotifyActivity.OnDisabled += gCrosshairNotifyDisabled;
        gCrosshairNotifyActivity.OnEnabled += gCrosshairNotifyEnabled;
    }

    private void OnDestroy() {
        incNotifyActivity.OnDisabled -= incNotifyDisabled;
        gCrosshairNotifyActivity.OnDisabled -= gCrosshairNotifyDisabled;
        gCrosshairNotifyActivity.OnEnabled -= gCrosshairNotifyEnabled;
    }

    #region Events

    private void incNotifyDisabled() => currentIncValue = 0;

    private void gCrosshairNotifyDisabled() => crosshairState = 0;

    private void gCrosshairNotifyEnabled() => crosshairState = 1;

    #endregion

    /// <summary>Display a temporary increase text to focus.</summary>
    /// <param name="value">Value to display. If a value is already displaying then it's added.</param>
    public void ShowFocusIncrease(float value) {
        currentIncValue += value;
        _incNotification.text = "+" + currentIncValue;
        if (_incNotification.gameObject.activeSelf)
            _incNotification.gameObject.SetActive(false);

        _incNotification.gameObject.SetActive(true);
    }

    public void UpdateMomentumUI(float value) {
        float r_val = (float)Math.Round(value, 3, MidpointRounding.AwayFromZero);
        if (_momentumValueOverlay != null) _momentumValueOverlay.text = "Momentum: x" + r_val.ToString("F3");
        else Debug.LogWarning(this + " Speed Mult Display is null");
    }

    public void UpdateCrosshair(float rawMomentum) {
        if (crosshairState == 2) return;
        if (crosshairState == 0) {
            if (rawMomentum > lastRawMomentum) {
                if (crosshairArrowState != 1) {
                    crosshairArrowState = 1;
                    SetCrosshair("▲");
                }
            } else if (rawMomentum < lastRawMomentum) {
                if (crosshairArrowState != -1) {
                    crosshairArrowState = -1;
                    SetCrosshair("▼");
                }
            } else {
                if (crosshairArrowState != 0) {
                    crosshairArrowState = 0;
                    SetCrosshair("●");
                }
            }
            lastRawMomentum = rawMomentum;
        } else { // crosshair state == 1
            crosshairState = 2;
            crosshairArrowState = 2; // reset the crosshair
            SetCrosshair("");
        }
    }

    private void SetCrosshair(string s) {
        if (_crosshair != null) _crosshair.text = s;
        else Debug.LogWarning(this + " Inc/Dec Display is null");
    }

    public void UpdateSpeedBar(float value, bool instant = false) {
        if (_focusBar == null) {
            Debug.LogWarning(this + " Momentum Bar is null");
            return;
        }
        if (instant) _focusBar.value = value;
        else _focusBar.value = Mathf.MoveTowards(_focusBar.value, value, Time.fixedDeltaTime * 25f);
        if (_focusValue == null) {
            Debug.LogWarning(this + " Speed Stat Value is null");
            return;
        }
        _focusValue.text = Mathf.Round(Player.Instance.CurrentSpeedStat) + "/" + Mathf.Round(Player.Instance.MaxSpeedStat);
    }

    public void ToggleBuffOverlay(bool toggle, float multuplier) {
        _momentumUp.SetActive(toggle);
        if (_buffText != null) _buffText.text = "Gain Up! [x" + multuplier + "]";
    }

    public void ActionFailed() => _momentumBarAnimator.SetTrigger("Flash");

}