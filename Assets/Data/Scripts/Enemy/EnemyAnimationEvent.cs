using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour {

    private AttackDetection attackDetection;

    private void Start() {
        attackDetection = GetComponentInChildren<AttackDetection>();
    }

    public void TriggerAttackBox() {
        attackDetection.TriggerAttackBox();
    }
}
