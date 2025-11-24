using System;
using UnityEngine;

public class UtilDisableOnEvent : MonoBehaviour {

    [SerializeField] private bool disableOnStart = true;

    public Action OnDisable;
    public Action OnEnable;

    public bool IsEnabled { get; private set; }

    private void Start() {
        if (disableOnStart) SetDisabled();
    }


    public void SetDisabled() {
        OnDisable?.Invoke();
        gameObject.SetActive(false);
    }
    public void SetEnabled() {
        OnEnable?.Invoke();
        gameObject.SetActive(true);
    }
}
