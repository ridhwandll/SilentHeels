using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boosts/StrongMelee")]
public class StrongMelee : Ability
{
    [Header("Boost Stats")]
    public int MeleeAttackDamageMultiplier = 2;

    public override void Activate()
    {
        PlayerData.Instance.Data.MeleeAttackDamageMultiplier += MeleeAttackDamageMultiplier;
    }

    public override void Deactivate()
    {
        PlayerData.Instance.Data.MeleeAttackDamageMultiplier -= MeleeAttackDamageMultiplier;
    }
}
