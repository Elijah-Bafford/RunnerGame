using System;
using UnityEngine;

public class UtilObjectActiveEvent : MonoBehaviour {

    [SerializeField] private bool disableOnStart = true;
    [Tooltip("Events are only sent by manual enable disable events. ex: animation event calls will send events NOT Unity onEnable/Disable events")]
    [SerializeField] private bool conditionalEvents = true;

    public Action OnDisabled;
    public Action OnEnabled;

    public bool IsEnabled { get; private set; }

    private void Start() { if (disableOnStart) SetDisabled(); }
    private void OnEnable() { if (!conditionalEvents) OnEnabled?.Invoke(); }
    private void OnDisable() { if (!conditionalEvents) OnDisabled?.Invoke(); }

    public void SetDisabled() {
        if (conditionalEvents) OnDisabled?.Invoke();
        gameObject.SetActive(false);
    }
    public void SetEnabled() {
        if (conditionalEvents) OnEnabled?.Invoke();
        gameObject.SetActive(true);
    }
}
