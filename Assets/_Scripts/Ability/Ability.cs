using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string AbilityName;
    public Sprite Icon;
    public float CooldownTime = 10f;
    public float ActiveDuration = 10f;

    public abstract void Activate();
    public abstract void Deactivate();
}