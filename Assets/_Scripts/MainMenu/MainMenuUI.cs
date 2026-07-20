using UnityEngine;
using UnityEngine.UIElements;

namespace MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        public SoundMixerManager soundMixerManager;
        public UIDocument uiDocument;

        private Button _playButton;
        private Button _optionsButton;
        private Button _exitButton;

        // OPTIONS MENU
        private Slider _masterVolSlider;
        private Slider _soundFXVolSlider;
        private Slider _musicVolSlider;

        private OptionsMenuManager _optionsMenuManager;
        private VisualElement _optionsMenu;
        private VisualElement _mainMenu;
        private VisualElement _shopMenu;

        private Button _backToMainMenuButton;
        private Button _audioOptionsButton;

        // Difficulty
        private RadioButtonGroup _difficultyOptionsGroup;

        private Label _highscoreLabel;
        public AudioClip buttonClickSound;
        public AudioClip buyButtonClickSound;

        void Start()
        {
            VisualElement root = uiDocument.rootVisualElement;

            _optionsMenuManager = new OptionsMenuManager();
            _optionsMenuManager.Initialize(uiDocument, buttonClickSound);

            _highscoreLabel = root.Q<Label>("Highscore");
            _optionsMenu = root.Q<VisualElement>("OptionsMenu");
            _mainMenu = root.Q<VisualElement>("MainMenu");
            _shopMenu = root.Q<VisualElement>("ShopMenu");

            _backToMainMenuButton = root.Q<Button>("BackButton");
            _playButton = root.Q<Button>("PlayButton1");
            _optionsButton = root.Q<Button>("OptionsButton");
            _exitButton = root.Q<Button>("ExitButton");
            _audioOptionsButton = root.Q<Button>("AudioButton");

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

            _playButton.clicked += OnPlayButtonPressed;
            _optionsButton.clicked += OnOptionsButtonPressed;
            _exitButton.clicked += OnExitButtonPressed;
            _backToMainMenuButton.clicked += OnBackToMainMenuButtonPressed;
            _audioOptionsButton.clicked += () => { _optionsMenuManager.Show(MenuType.AudioMenu); };

            _mainMenu.style.display = DisplayStyle.Flex;
            _optionsMenu.style.display = DisplayStyle.None;

            //PlayerProgress.Instance.Load();
            //_highscoreLabel.text = "HIGHSCORE: " + Globals.Highscore;
        }

        private void OnPlayButtonPressed()
        {
            PlayButtonPressedSound();
            GameManager.Instance.LoadGame();
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
        //// AUDIO ////
        private void OnMasterVolumeChanged(ChangeEvent<float> e)
        {
            soundMixerManager.SetMasterVolume(e.newValue);
        }
        private void OnSoundFXVolumeChanged(ChangeEvent<float> e)
        {
            soundMixerManager.SetSoundFXVolume(e.newValue);
        }
        private void OnMusicVolumeChanged(ChangeEvent<float> e)
        {
            soundMixerManager.SetMusicVolume(e.newValue);
        }

        private void OnBackToMainMenuButtonPressed()
        {
            _optionsMenu.style.display = DisplayStyle.None;
            _backToMainMenuButton.style.display = DisplayStyle.None;
            PlayerData.Instance.Save();
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
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, 0.5f);
        }
    }
}
