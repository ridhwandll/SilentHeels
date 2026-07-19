using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Health Bar
    [Header("Health Bar")]
    public Slider healthSlider;
    public TMP_Text healthText;
    public Image healthBarBorder;

    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject deathMenu;

    [Header("Tips")]
    public float showDuration = 7f;
    public float fadeOutDuration = 2f;

    public AudioClip buttonClickSound;

    void Start()
    {
        PlayerCombat playerObj = GameObject.FindWithTag("Player").GetComponent<PlayerCombat>();
        playerObj.OnHealthChanged += UpdatePlayerHealth;
    }

    public void UpdatePlayerHealth(int playerHealth, int maxHealth)
    {
        healthSlider.value = playerHealth;
        healthText.text = playerHealth + "/" + maxHealth;

        if (playerHealth <= maxHealth / 3)
        {
            healthBarBorder.color = Color.softRed;
            healthText.color = Color.red;
        }
        else
        {
            healthBarBorder.color = Color.gray1;
            healthText.color = Color.gray1;
        }
    }

    private void OnPauseChanged(bool paused)
    {
        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.9f);

        deathMenu.SetActive(false);
        pauseMenu.SetActive(paused);
    }

    private void OnPlayerDied()
    {
        pauseMenu.SetActive(false);
        deathMenu.SetActive(true);
        //PlayerProgress.Instance.Save();
    }

    public void OnReturnToMMPressed()
    {
        if (SoundFXManager.instance)
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.5f);

        GameObject.FindGameObjectWithTag("LevelTransition").GetComponent<LevelTransition>().LoadMainMenu();
    }

    IEnumerator ShowFadeInAndOut(TMP_Text text)
    {
        text.gameObject.SetActive(true);

        Color c = text.color;
        c.a = 0f;
        text.color = c;
        float maxAlpha = 0.7f;

        // FadeIN
        float t = 0f;
        float fadeInDuration = 2f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, t / fadeInDuration);
            text.color = c;
            yield return null;
        }

        // Ensure fully visible
        c.a = maxAlpha;
        text.color = c;

        // Stay visible
        yield return new WaitForSeconds(showDuration);

        // Fade OUT
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeOutDuration);
            text.color = c;
            yield return null;
        }

        // Ensure invisible & disable
        c.a = 0f;
        text.color = c;
        text.gameObject.SetActive(false);
    }
}
