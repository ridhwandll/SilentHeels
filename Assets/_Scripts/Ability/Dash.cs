using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boosts/Dash")]
public class Dash : Ability
{
    [Header("Boost Stats")]
    public float ExtraDashSpeedMultiplier = 1.0f;
    public float ExtraDashDuration = 0.0f;

    public override void Activate()
    {
        PlayerData.Instance.Data.CanDash = true;
        PlayerData.Instance.Data.DashForceMultiplier += ExtraDashSpeedMultiplier;
        PlayerData.Instance.Data.DashDuration += ExtraDashDuration;
    }

    public override void Deactivate()
    {
        PlayerData.Instance.Data.CanDash = false;
        PlayerData.Instance.Data.DashForceMultiplier -= ExtraDashSpeedMultiplier;
        PlayerData.Instance.Data.DashDuration -= ExtraDashDuration;
    }
}