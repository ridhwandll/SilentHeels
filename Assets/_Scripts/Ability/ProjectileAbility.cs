using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boosts/ProjectileAbility")]
public class ProjectileAbility : Ability
{
    [Header("Boost Stats")]
    public float RangeAttackSpeedMultiplier = 1.0f;
    public int RangeAttackDamageMultiplier = 1;

    public override void Activate()
    {
        PlayerData.Instance.Data.CanRangeAttack = true;
        PlayerData.Instance.Data.RangeAttackSpeedMultiplier += RangeAttackSpeedMultiplier;
        PlayerData.Instance.Data.RangeAttackDamageMultiplier += RangeAttackDamageMultiplier;
    }

    public override void Deactivate()
    {
        PlayerData.Instance.Data.CanRangeAttack = false;
        PlayerData.Instance.Data.RangeAttackSpeedMultiplier -= RangeAttackSpeedMultiplier;
        PlayerData.Instance.Data.RangeAttackDamageMultiplier -= RangeAttackDamageMultiplier;
    }
}