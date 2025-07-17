using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {

    [Header("Animation Event Handler Scripts")]
    [Tooltip("Weapon Script")]
    [SerializeField] private PlayerAttack attack;
    [Tooltip("Player Script")]
    [SerializeField] Player player;

    public void EnableWeapon() { attack.EnableWeapon(); }
    public void DisableWeapon() { attack.DisableWeapon(); }
    public void AllowAttack() { player.AllowAttack(); }
    
}
