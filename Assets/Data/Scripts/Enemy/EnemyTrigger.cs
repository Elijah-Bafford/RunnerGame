using UnityEngine;
/// <summary>
/// The EnemyTrigger class is used as to detect if the player is in either aggro range or attack range.
/// This class is attached to the enemy colliders, and the TriggerType is assigned to the respective collider.
/// </summary>
public class EnemyTrigger : MonoBehaviour {
    [System.Serializable] public enum TriggerType { Aggro, Attack }
    [Header("The collider this instance is attached to.")]
    [SerializeField] private TriggerType triggerType;
    [Header("Refs")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Enemy enemy;

    private void OnTriggerEnter(Collider collision) {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0) {
            if (triggerType == TriggerType.Attack) {
                enemy.SetState(Enemy.State.Attack);
                enemy.SetInRangeForAttack(true);
            } else if (triggerType == TriggerType.Aggro) { enemy.SetState(Enemy.State.Move); }
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0) {
            if (triggerType == TriggerType.Attack) {
                enemy.SetState(Enemy.State.Move);
                enemy.SetInRangeForAttack(false);
            }
        }
    }
}
