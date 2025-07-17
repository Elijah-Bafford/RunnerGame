using UnityEngine;

public class EnemyTrigger : MonoBehaviour {
    [System.Serializable] public enum TriggerType { Aggro, Attack }
    [SerializeField] private TriggerType triggerType;
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private Enemy enemy;

    private void OnTriggerEnter(Collider collision) {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0) {
            if (triggerType == TriggerType.Attack) {
                enemy.SetState(Enemy.State.Attack);
            } else if (triggerType == TriggerType.Aggro){
                enemy.SetState(Enemy.State.Move);
            }
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0) {
            if (triggerType == TriggerType.Attack) {
                enemy.SetState(Enemy.State.Move);
            } else if (triggerType == TriggerType.Aggro) {
                enemy.SetState(Enemy.State.Idle);
            }
        }
    }
}
