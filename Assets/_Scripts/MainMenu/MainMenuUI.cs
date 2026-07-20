using UnityEngine;
using UnityEngine.UIElements;

namespace MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        public SoundMixerManager soundMixerManager;
        public UIDocument uiDocument;

        [Header("Button Images")]
        public Texture2D lockedLevelIcon;
        public Texture2D level1UnlockedIcon;
        public Texture2D level2UnlockedIcon;
        public Texture2D level3UnlockedIcon;

        private Button _playButton1;
        private Button _playButton2;
        private Button _playButton3;
        private Button _optionsButton;
        private Button _exitButton;

        // OPTIONS MENU
        private Slider _masterVolSlider;
        private Slider _soundFXVolSlider;
        private Slider _musicVolSlider;

        private OptionsMenuManager _optionsMenuManager;
        private VisualElement _optionsMenu;
        private VisualElement _mainMenu;
        private VisualElement _root;

        private Button _backToMainMenuButton;
        private Button _audioOptionsButton;
        private Button _howToPlayButton;

        public AudioClip buttonClickSound;
        public AudioClip buyButtonClickSound;

        void Start()
        {
            VisualElement root = uiDocument.rootVisualElement;

            _optionsMenuManager = new OptionsMenuManager();
            _optionsMenuManager.Initialize(uiDocument, buttonClickSound);

            _root = root.Q<VisualElement>("Root");
            _optionsMenu = root.Q<VisualElement>("OptionsMenu");
            _mainMenu = root.Q<VisualElement>("MainMenu");

            _backToMainMenuButton = root.Q<Button>("BackButton");
            _optionsButton = root.Q<Button>("OptionsButton");
            _exitButton = root.Q<Button>("ExitButton");
            _audioOptionsButton = root.Q<Button>("AudioButton");
            _howToPlayButton = root.Q<Button>("HowToPlayButton");

            _playButton1 = root.Q<Button>("PlayButton1");
            _playButton2 = root.Q<Button>("PlayButton2");
            _playButton3 = root.Q<Button>("PlayButton3");

            _masterVolSlider = root.Q<Slider>("MasterVol");
            _soundFXVolSlider = root.Q<Slider>("SoundFXVol");
            _musicVolSlider = root.Q<Slider>("MusicVol");

            //// OPTIONS-AUDIO
            _masterVolSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
            _soundFXVolSlider.RegisterValueChangedCallback(OnSoundFXVolumeChanged);
            _musicVolSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
            _masterVolSlider.value = 1.0f;
            _soundFXVolSlider.value = 1.0f;
            _musicVolSlider.value = 0.5f;

            // Fetch the unlocked level from our JSON Singleton
            int unlockedLevel = 1;
            if (PlayerData.Instance != null && PlayerData.Instance.Data != null)
            {
                //unlockedLevel = PlayerData.Instance.Data.HighestLevelUnlocked;
                unlockedLevel = 1;
            }

            // Setup buttons dynamically, passing the specific unlocked image for each level
            SetupLevelButton(_playButton1, 1, unlockedLevel, "Level_1", level1UnlockedIcon);
            SetupLevelButton(_playButton2, 2, unlockedLevel, "Level_2", level2UnlockedIcon);
            SetupLevelButton(_playButton3, 3, unlockedLevel, "Level_3", level3UnlockedIcon);

            _optionsButton.clicked += OnOptionsButtonPressed;
            _exitButton.clicked += OnExitButtonPressed;
            _backToMainMenuButton.clicked += OnBackToMainMenuButtonPressed;
            _audioOptionsButton.clicked += () => { _optionsMenuManager.Show(MenuType.AudioMenu); };
            _howToPlayButton.clicked += () => { _optionsMenuManager.Show(MenuType.HowToPlayMenu); };

            _mainMenu.style.display = DisplayStyle.Flex;
            _optionsMenu.style.display = DisplayStyle.None;
        }

        private void SetupLevelButton(Button btn, int levelRequirement, int currentlyUnlocked, string sceneName, Texture2D unlockedImage)
        {
            if (btn == null)
                return;

            if (currentlyUnlocked >= levelRequirement)
            {
                // Unlocked State
                btn.SetEnabled(true);
                btn.style.backgroundImage = new StyleBackground(unlockedImage);

                btn.clicked += delegate
                {
                    PlayButtonPressedSound();
                    GameManager.Instance.LoadGame(sceneName);
                    _root.style.backgroundImage = new StyleBackground(unlockedImage);
                };
            }
            else
            {
                // Locked State
                btn.SetEnabled(false);
                btn.style.backgroundImage = new StyleBackground(lockedLevelIcon);
            }
        }

        private void OnOptionsButtonPressed()
        {
            PlayButtonPressedSound();
            _backToMainMenuButton.style.display = DisplayStyle.Flex;
            _mainMenu.style.display = DisplayStyle.None;
            AnimateAndShowMenu(_optionsMenu);
        }

        private void OnExitButtonPressed()
        {
            PlayButtonPressedSound();
            Application.Quit();
        }

        //////////// OPTIONS MENU ////////////
        private void OnMasterVolumeChanged(ChangeEvent<float> e) { soundMixerManager.SetMasterVolume(e.newValue); }
        private void OnSoundFXVolumeChanged(ChangeEvent<float> e) { soundMixerManager.SetSoundFXVolume(e.newValue); }
        private void OnMusicVolumeChanged(ChangeEvent<float> e) { soundMixerManager.SetMusicVolume(e.newValue); }

        private void OnBackToMainMenuButtonPressed()
        {
            _optionsMenu.style.display = DisplayStyle.None;
            _backToMainMenuButton.style.display = DisplayStyle.None;

            if (PlayerData.Instance != null) PlayerData.Instance.Save();

            AnimateAndShowMenu(_mainMenu);
            PlayButtonPressedSound();
            GameManager.Instance.LoadMainMenu();
        }

        private void AnimateAndShowMenu(VisualElement menuToShow)
        {
            menuToShow.style.display = DisplayStyle.Flex;
            menuToShow.style.opacity = 0f;
            menuToShow.style.scale = Vector3.one * 0.95f;

            menuToShow.experimental.animation
                .Start(0f, 1f, 369, (VisualElement m, float value) =>
                {
                    menuToShow.style.opacity = value;
                    menuToShow.style.scale = Vector3.one * Mathf.Lerp(0.95f, 1f, value);
                });
        }

        private void PlayButtonPressedSound()
        {
            if (SoundFXManager.instance != null)
            {
                SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.5f);
            }
        }
    }
}