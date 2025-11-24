using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MomentumUI : MonoBehaviour {

    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI _momentumValueOverlay;
    [SerializeField] private GameObject _buffOverlay;
    [SerializeField] private Slider _speedStatBar;
    [SerializeField] private TextMeshProUGUI _incDec;
    [SerializeField] private TextMeshProUGUI _incNotification;
    [SerializeField] private TextMeshProUGUI _speedStatValue;

    private TextMeshProUGUI _buffText;

    private Animator _momentumBarAnimator;

    private float currentIncValue = 0;

    public static MomentumUI Instance { get; private set; }

    private void Start() {
        Instance = this; // temp // until I decided how I want to handle UI handlers
        _momentumBarAnimator = _speedStatBar.GetComponent<Animator>();
        _buffText = _buffOverlay.GetComponent<TextMeshProUGUI>();
        _incNotification.gameObject.GetComponent<UtilDisableOnEvent>().OnDisable += ResetCurrentIncValue;
    }

    private void OnDestroy() => _incNotification.gameObject.GetComponent<UtilDisableOnEvent>().OnDisable -= ResetCurrentIncValue;

    private void ResetCurrentIncValue() => currentIncValue = 0;

    public void TriggerSSIncrease(float value) {
        currentIncValue += value;
        _incNotification.text = "+" + currentIncValue;
        if (_incNotification.gameObject.activeSelf)
            _incNotification.gameObject.SetActive(false);

        _incNotification.gameObject.SetActive(true);
    }

    public void UpdateSpeedMult(float value) {
        float r_val = (float)Math.Round(value, 3, MidpointRounding.AwayFromZero);
        if (_momentumValueOverlay != null) _momentumValueOverlay.text = "Momentum: x" + r_val.ToString("F3");
        else Debug.LogWarning(this + " Speed Mult Display is null");
    }

    float m_temp = 0f;
    sbyte m_temp2 = 0;

    public void UpdateCrosshair(float speedMult) {
        if (speedMult > m_temp) {
            if (m_temp2 != 1) {
                m_temp2 = 1;
                SetCrosshair("▲");
            }
        } else if (speedMult < m_temp) {
            if (m_temp2 != -1) {
                m_temp2 = -1;
                SetCrosshair("▼");
            }
        } else {
            if (m_temp2 != 0) {
                m_temp2 = 0;
                SetCrosshair("-");
            }
        }
        m_temp = speedMult;
    }

    private void SetCrosshair(string s) {
        if (_incDec != null) _incDec.text = s;
        else Debug.LogWarning(this + " Inc/Dec Display is null");
    }

    public void UpdateSpeedBar(float value, bool instant = false) {
        if (_speedStatBar == null) {
            Debug.LogWarning(this + " Momentum Bar is null");
            return;
        }
        if (instant) _speedStatBar.value = value;
        else _speedStatBar.value = Mathf.MoveTowards(_speedStatBar.value, value, Time.fixedDeltaTime * 25f);
        if (_speedStatValue == null) {
            Debug.LogWarning(this + " Speed Stat Value is null");
            return;
        }
        _speedStatValue.text = Mathf.Round(Player.Instance.CurrentSpeedStat) + "/" + Mathf.Round(Player.Instance.MaxSpeedStat);
    }

    public void ToggleBuffOverlay(bool toggle, float multuplier) {
        _buffOverlay.SetActive(toggle);
        if (_buffText != null) _buffText.text = "(x" + multuplier + ")";
    }

    public void ActionFailed() => _momentumBarAnimator.SetTrigger("Flash");

}