using System.Collections;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public AbilityDatabase Database;
    private UIManager _uiManager;

    [System.Serializable]
    public class AbilitySlot
    {
        public KeyCode InputKey;
        public Ability EquippedAbility;
        [HideInInspector] public float currentCooldown;
        [HideInInspector] public bool isOnCooldown;
        [HideInInspector] public bool isActive;
    }

    public AbilitySlot SlotQ = new AbilitySlot { InputKey = KeyCode.Q };
    public AbilitySlot SlotE = new AbilitySlot { InputKey = KeyCode.E };

    void Start()
    {
        _uiManager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIManager>();
    }

    public void SetQAbility()
    {
        string q_id = PlayerData.Instance.Data.EquippedQ_Name;
        SlotQ.EquippedAbility = Database.GetAbilityByName(q_id);
        //if (SlotQ.EquippedAbility != null)
        //    _uiManager.SetupUI(SlotQ);
    }
    public void SetEAbility()
    {
        string e_id = PlayerData.Instance.Data.EquippedE_Name;
        SlotE.EquippedAbility = Database.GetAbilityByName(e_id);
        //if (SlotE.EquippedAbility != null)
        //    _uiManager.SetupUI(SlotE);
    }

    void Update()
    {
        ProcessSlot(SlotQ);
        ProcessSlot(SlotE);
    }

    private void ProcessSlot(AbilitySlot slot)
    {
        if (slot.EquippedAbility == null)
            return;

        if (slot.isOnCooldown)
        {
            slot.currentCooldown -= Time.deltaTime;
            _uiManager.UpdateCooldownFill(slot.InputKey, slot.currentCooldown / slot.EquippedAbility.CooldownTime);

            if (slot.currentCooldown <= 0f)
            {
                slot.isOnCooldown = false;
                slot.currentCooldown = 0f;
            }
        }

        if (Input.GetKeyDown(slot.InputKey) && !slot.isOnCooldown && !slot.isActive)
        {
            Debug.Log($"Activating ability: {slot.EquippedAbility.AbilityName}");
            StartCoroutine(HandleAbilityRoutine(slot));
        }
    }

    private IEnumerator HandleAbilityRoutine(AbilitySlot slot)
    {
        slot.isActive = true;
        slot.EquippedAbility.Activate();
        _uiManager.SetAbilityActiveState(slot.InputKey, true);
        ParticleSystem abilityPS = gameObject.GetComponent<PlayerCombat>().GetAbilityPS();
        abilityPS.Play();

        yield return new WaitForSeconds(slot.EquippedAbility.ActiveDuration);

        slot.EquippedAbility.Deactivate();
        slot.isActive = false;
        slot.isOnCooldown = true;
        slot.currentCooldown = slot.EquippedAbility.CooldownTime;
        _uiManager.SetAbilityActiveState(slot.InputKey, false);
        abilityPS.Stop();
    }
}