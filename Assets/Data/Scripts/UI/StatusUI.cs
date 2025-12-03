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

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _momentumBarAnimator = _focusBar.GetComponent<Animator>();
        _buffText = _momentumUp.GetComponent<TextMeshProUGUI>();
        incNotifyActivity = _incNotification.gameObject.GetComponent<UtilObjectActiveEvent>();
        gCrosshairNotifyActivity = _grappleReticle.gameObject.GetComponent<UtilObjectActiveEvent>();
        GameStateHandler.OnLevelRestart += OnLevelRestart;
        incNotifyActivity.OnDisabled += incNotifyDisabled;
        gCrosshairNotifyActivity.OnDisabled += gCrosshairNotifyDisabled;
        gCrosshairNotifyActivity.OnEnabled += gCrosshairNotifyEnabled;
        Player.Instance.OnMaxFocusChanged += SetMaxValueFocusBar;
    }

    private void OnDestroy() {
        GameStateHandler.OnLevelRestart -= OnLevelRestart;
        incNotifyActivity.OnDisabled -= incNotifyDisabled;
        gCrosshairNotifyActivity.OnDisabled -= gCrosshairNotifyDisabled;
        gCrosshairNotifyActivity.OnEnabled -= gCrosshairNotifyEnabled;
    }

    #region Events

    private void incNotifyDisabled() => currentIncValue = 0;

    private void gCrosshairNotifyDisabled() => crosshairState = 0;

    private void gCrosshairNotifyEnabled() => crosshairState = 1;

    #endregion

    #region Crosshair

    /// <summary>Display momentum change as crosshair.</summary>
    /// <param name="rawMomentum">The raw momentum.</param>
    public void UpdateCrosshair(float rawMomentum) {
        if (crosshairState == 2) return;

        if (crosshairState == 1) {
            crosshairState = 2;
            crosshairArrowState = 2;     // reset the crosshair
            SetCrosshairText("");
            return;
        }

        sbyte newArrowState = 0;
        string newText = "●";

        if (rawMomentum > lastRawMomentum) {
            newArrowState = 1;
            newText = "▲";
        } else if (rawMomentum < lastRawMomentum) {
            newArrowState = -1;
            newText = "▼";
        }

        if (crosshairArrowState != newArrowState) {
            crosshairArrowState = newArrowState;
            SetCrosshairText(newText);
        }

        lastRawMomentum = rawMomentum;
    }

    private void SetCrosshairText(string s) => _crosshair.text = s;

    #endregion

    #region Focus Bar
    /// <summary>Display a temporary increase text to focus.</summary>
    /// <param name="value">Value to display. If a value is already displaying then it's added.</param>
    public void ShowFocusIncrease(float value) {
        currentIncValue += value;
        _incNotification.text = "+" + currentIncValue;
        if (_incNotification.gameObject.activeSelf)
            _incNotification.gameObject.SetActive(false);

        _incNotification.gameObject.SetActive(true);
    }
    /// <summary>Update the Focus Bar fill</summary>
    /// <param name="value">The target value.</param>
    /// <param name="instant">Instantly fill or MoveTowards.</param>
    public void UpdateFocusBar(float value, bool instant = false) {
        if (instant) _focusBar.value = value;
        else _focusBar.value = Mathf.MoveTowards(_focusBar.value, value, Time.fixedDeltaTime * 25f);

        _focusValue.text = Mathf.Round(Player.Instance.CurrentFocus) + "/" + Mathf.Round(Player.Instance.MaxFocus);
    }
    /// <summary>Set the max Focus Bar value.</summary>
    /// <param name="value">New max value.</param>
    public void SetMaxValueFocusBar(float value) => _focusBar.maxValue = value;
    

    /// <summary>Flash the focus bar background red.</summary>
    public void FlashFocusBar() => _momentumBarAnimator.SetTrigger("Flash");

    #endregion

    #region Momentum UI Status
    public void UpdateMomentumUI(float value) {
        float r_val = (float)Math.Round(value, 3, MidpointRounding.AwayFromZero);
        _momentumValueOverlay.text = "Momentum: x" + r_val.ToString("F3");
    }

    /// <summary>Show Momentum Status Up arrow and multiplier.</summary>
    /// <param name="show">Show the menu or not.</param>
    /// <param name="multuplier">The multiplier to display.</param>
    public void SetMomentumUpOverlayActive(bool show, float multuplier) {
        _momentumUp.SetActive(show);
        if (_buffText != null) _buffText.text = "▲ x" + ((float)Math.Round(multuplier, 1, MidpointRounding.AwayFromZero)) + "";
    }

    #endregion

    private void OnLevelRestart() => SetMomentumUpOverlayActive(false, 1);
    
}