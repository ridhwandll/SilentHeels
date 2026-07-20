using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boosts/Kickoff")]
public class Kickoff : Ability
{
    [Header("Boost Stats")]
    public float MoveSpeedMultiplier = 1.0f;
    public float JumpForceMultiplier = 1.0f;
    public int ExtraJumps = 1;

    public override void Activate()
    {
        PlayerData.Instance.Data.MoveSpeedMultiplier += MoveSpeedMultiplier;
        PlayerData.Instance.Data.JumpForceMultiplier += JumpForceMultiplier;
        PlayerData.Instance.Data.ExtraJumps += ExtraJumps;
    }

    public override void Deactivate()
    {
        PlayerData.Instance.Data.MoveSpeedMultiplier -= MoveSpeedMultiplier;
        PlayerData.Instance.Data.JumpForceMultiplier -= JumpForceMultiplier;
        PlayerData.Instance.Data.ExtraJumps -= ExtraJumps;
    }
}
