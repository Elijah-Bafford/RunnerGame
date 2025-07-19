using UnityEngine;

public class AllowHitEvent : MonoBehaviour {
    [SerializeField] private Enemy enemy;

    public void AllowHit() { enemy.AllowHit(); }
}