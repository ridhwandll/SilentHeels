using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class LoadoutButtonConfig
    {
        public string AbilityName;
        public GameObject ButtonGameObject;
        public Image ButtonIcon;

        public GameObject SelectionBorder;
        public TMP_Text ButtonText;
    }

    [Header("Loadout Menu Setup")]
    public List<LoadoutButtonConfig> LoadoutButtons = new List<LoadoutButtonConfig>();
    public Sprite LockedIconSprite;

    [Header("Health Bar")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public Image healthBarBorder;

    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject deathMenu;
    public GameObject loadoutMenu;

    [Header("Tips")]
    public float showDuration = 7f;
    public float fadeOutDuration = 2f;

    public AudioClip buttonClickSound;
    public AbilityDatabase Database;

    [Header("HUD Ability Slots")]
    public Image ImageSlotQ;
    public Image CooldownOverlayQ;
    public Image ImageSlotE;
    public Image CooldownOverlayE;

    // Breathing Effect
    [Header("Active Breathing Effect")]
    public float breathingSpeed = 10f;
    public float breathingScale = 0.2f;

    private bool isQActive = false;
    private bool isEActive = false;

    private bool isPaused = false;

    void Start()
    {
        PlayerCombat playerObj = GameObject.FindWithTag("Player").GetComponent<PlayerCombat>();
        playerObj.OnHealthChanged += UpdatePlayerHealth;
        playerObj.OnPlayerDied += OnPlayerDied;

        // Grants all abilities in your UI list to the player's save data
        foreach (var config in LoadoutButtons)
        {
            if (!PlayerData.Instance.Data.UnlockedAbilities.Contains(config.AbilityName))
                PlayerData.Instance.Data.UnlockedAbilities.Add(config.AbilityName);
        }

        PlayerData.Instance.Save();
        CheckAndShowLoadout();
    }

    void Update()
    {
        // Breathing effect
        if (isQActive && ImageSlotQ != null)
        {
            float scale = 1f + ((Mathf.Sin(Time.time * breathingSpeed) + 1f) / 2f) * breathingScale;
            ImageSlotQ.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
        else if (ImageSlotQ != null)
            ImageSlotQ.rectTransform.localScale = Vector3.one; // Snap back to normal

        if (isEActive && ImageSlotE != null)
        {
            float scale = 1f + ((Mathf.Sin(Time.time * breathingSpeed) + 1f) / 2f) * breathingScale;
            ImageSlotE.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
        else if (ImageSlotE != null)
        {
            ImageSlotE.rectTransform.localScale = Vector3.one;
        }

        if (deathMenu.activeSelf || loadoutMenu.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            OnPauseChanged(isPaused);
        }
    }

    private void OnPauseChanged(bool paused)
    {
        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.9f);

        pauseMenu.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    private void CheckAndShowLoadout()
    {
        // Because we unlocked everything in Start(), this will safely trigger every time!
        if (PlayerData.Instance.Data.UnlockedAbilities.Count > 2)
        {
            Time.timeScale = 0f;
            loadoutMenu.SetActive(true);

            PlayerData.Instance.Data.EquippedQ_Name = "";
            PlayerData.Instance.Data.EquippedE_Name = "";
            SetupLoadoutMenu();
        }
        else
        {
            // Fallback just in case
            AutoEquipEarlyAbilities();
            loadoutMenu.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void SetupLoadoutMenu()
    {
        foreach (var config in LoadoutButtons)
        {
            config.ButtonGameObject.SetActive(true);

            if (config.ButtonText != null)
                config.ButtonText.color = Color.white;

            if (PlayerData.Instance.Data.UnlockedAbilities.Contains(config.AbilityName))
            {
                Ability ab = Database.GetAbilityByName(config.AbilityName);
                if (ab != null)
                    config.ButtonIcon.sprite = ab.Icon;

                config.ButtonIcon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                if (config.SelectionBorder != null) config.SelectionBorder.SetActive(false);
            }
            else
            {
                config.ButtonIcon.sprite = LockedIconSprite;
                config.ButtonIcon.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                if (config.SelectionBorder != null) config.SelectionBorder.SetActive(false);
            }
        }

        RefreshUI();
    }

    public void EquipAbility(string selectedAbilityID)
    {
        if (!PlayerData.Instance.Data.UnlockedAbilities.Contains(selectedAbilityID))
        {
            Debug.Log("That ability is locked!");
            return;
        }

        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.9f);

        if (PlayerData.Instance.Data.EquippedQ_Name == selectedAbilityID)
        {
            PlayerData.Instance.Data.EquippedQ_Name = "";
            RefreshLoadoutHighlights();
            return;
        }
        if (PlayerData.Instance.Data.EquippedE_Name == selectedAbilityID)
        {
            PlayerData.Instance.Data.EquippedE_Name = "";
            RefreshLoadoutHighlights();
            return;
        }

        if (string.IsNullOrEmpty(PlayerData.Instance.Data.EquippedQ_Name))
        {
            PlayerData.Instance.Data.EquippedQ_Name = selectedAbilityID;
            GameObject.FindWithTag("Player").GetComponent<AbilityManager>().SetQAbility();
        }
        else if (string.IsNullOrEmpty(PlayerData.Instance.Data.EquippedE_Name))
        {
            PlayerData.Instance.Data.EquippedE_Name = selectedAbilityID;
            GameObject.FindWithTag("Player").GetComponent<AbilityManager>().SetEAbility();
        }
        else
        {
            PlayerData.Instance.Data.EquippedQ_Name = selectedAbilityID;
        }

        RefreshLoadoutHighlights();

        if (!string.IsNullOrEmpty(PlayerData.Instance.Data.EquippedQ_Name) && !string.IsNullOrEmpty(PlayerData.Instance.Data.EquippedE_Name))
            StartCoroutine(WaitAndStartRound());
    }

    private void RefreshLoadoutHighlights()
    {
        foreach (var config in LoadoutButtons)
        {
            if (!config.ButtonGameObject.activeSelf)
                continue;

            if (!PlayerData.Instance.Data.UnlockedAbilities.Contains(config.AbilityName))
                continue;

            if (PlayerData.Instance.Data.EquippedQ_Name == config.AbilityName || PlayerData.Instance.Data.EquippedE_Name == config.AbilityName)
            {
                config.ButtonIcon.color = Color.white;
                if (config.SelectionBorder != null) config.SelectionBorder.SetActive(true);
            }
            else
            {
                config.ButtonIcon.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                if (config.SelectionBorder != null) config.SelectionBorder.SetActive(false);
            }

            if (config.ButtonText != null)
                config.ButtonText.color = Color.white;
        }
        RefreshUI();
    }

    private IEnumerator WaitAndStartRound()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        StartRound();
    }

    private void AutoEquipEarlyAbilities()
    {
        var unlocked = PlayerData.Instance.Data.UnlockedAbilities;

        if (unlocked.Count > 0 && string.IsNullOrEmpty(PlayerData.Instance.Data.EquippedQ_Name))
            PlayerData.Instance.Data.EquippedQ_Name = unlocked[0];

        if (unlocked.Count > 1 && string.IsNullOrEmpty(PlayerData.Instance.Data.EquippedE_Name))
            PlayerData.Instance.Data.EquippedE_Name = unlocked[1];

        RefreshUI();
    }

    public void StartRound()
    {
        loadoutMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RefreshUI()
    {
        if (PlayerData.Instance == null || PlayerData.Instance.Data == null || Database == null) return;

        if (CooldownOverlayQ != null)
            CooldownOverlayQ.fillAmount = 0f;
        if (CooldownOverlayE != null)
            CooldownOverlayE.fillAmount = 0f;

        Ability qAbility = Database.GetAbilityByName(PlayerData.Instance.Data.EquippedQ_Name);
        if (qAbility != null)
        {
            ImageSlotQ.sprite = qAbility.Icon;
            ImageSlotQ.color = Color.white;
        }
        else
        {
            ImageSlotQ.color = Color.clear;
        }

        Ability eAbility = Database.GetAbilityByName(PlayerData.Instance.Data.EquippedE_Name);
        if (eAbility != null)
        {
            ImageSlotE.sprite = eAbility.Icon;
            ImageSlotE.color = Color.white;
        }
        else
        {
            ImageSlotE.color = Color.clear;
        }
    }

    public void UpdatePlayerHealth(int playerHealth, int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = playerHealth;
        healthText.text = playerHealth + "/" + maxHealth;

        if (playerHealth <= maxHealth / 3)
        {
            healthBarBorder.color = Color.red;
            healthText.color = Color.red;
        }
        else
        {
            healthBarBorder.color = Color.gray;
            healthText.color = Color.gray;
        }
    }

    public void OnPlayerDied()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        deathMenu.SetActive(true);
    }

    public void OnReturnToMMPressed()
    {
        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.5f);

        Time.timeScale = 1f;
        GameManager.Instance.LoadMainMenu();
    }

    public void UpdateCooldownFill(KeyCode slotKey, float fillPercentage)
    {
        if (slotKey == KeyCode.Q && CooldownOverlayQ != null)
        {
            CooldownOverlayQ.fillAmount = fillPercentage;
        }
        else if (slotKey == KeyCode.E && CooldownOverlayE != null)
        {
            CooldownOverlayE.fillAmount = fillPercentage;
        }
    }
    public void SetAbilityActiveState(KeyCode slotKey, bool isActive)
    {
        if (slotKey == KeyCode.Q) isQActive = isActive;
        else if (slotKey == KeyCode.E) isEActive = isActive;
    }
}