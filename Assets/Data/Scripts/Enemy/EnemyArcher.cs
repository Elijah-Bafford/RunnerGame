using UnityEngine;

public class EnemyArcher : EnemyBase {

    [SerializeField] private Animator bowAnimator;
    [Header("Arrow References")]
    [Tooltip("The arrow that is always attached to the enemy. This is used as a reference when creating a new arrow.")]
    [SerializeField] private GameObject constrainedArrow;
    [SerializeField] private GameObject arrowPrefab;
    //[Tooltip("The transform for where the arrow is placed on instantiation")]
    //[SerializeField] private Transform arrowLoosePosition;

    private bool showConstrainedArrow = true;

    protected override void ActionMove() {
        base.ActionMove();
    }

    protected override bool ActionAttack() {
        if (!base.ActionAttack()) return false;
        return true;
    }
    public void TriggerBowDrawstring() => bowAnimator.SetBool("Draw", !bowAnimator.GetBool("Draw"));

    public void FireBow() {
        bowAnimator.SetTrigger("Loose");
        bowAnimator.SetBool("Draw", false);
    }

    public void CancelDraw() {
        bowAnimator.SetBool("Draw", false);
    }

    public void SetConstrainedArrow(bool active) {
        showConstrainedArrow = active;
        constrainedArrow.SetActive(active);
    }

    /// <summary>
    /// Called when the arrow should be released from the bow
    /// </summary>
    public Arrow CreateLoosedArrow() {
        Vector3 pos = constrainedArrow.transform.position;
        Quaternion rot = constrainedArrow.transform.rotation * arrowPrefab.transform.rotation;
        GameObject a = Instantiate(arrowPrefab, pos, rot);
        a.SetActive(true);
        return a.GetComponent<Arrow>();

    }

    public Animator GetBowAnimator() => bowAnimator;
}
