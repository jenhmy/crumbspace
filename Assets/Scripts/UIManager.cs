using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject lvlCompletePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text starsCountText;
    [SerializeField] private TMP_Text croissantsCountText;
    [SerializeField] private TMP_Text aliensCountText;
    [SerializeField] private TMP_Text levelScoreText;
    [SerializeField] private TMP_Text lvlScoreLabelText;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreNamesText;
    [SerializeField] private TMP_Text highScoreValuesText;
    [SerializeField] private TMP_Text winLevelScoreText;
    [SerializeField] private TMP_Text lvlText;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button nameButton;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button saveButton;
    [Header("Options")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button unmuteButton;
    [SerializeField] private GameObject muteImage;
    [SerializeField] private GameObject unmuteImage;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button spanishButton;
    [SerializeField] private Button englishButton;
    

    private Color spanishButtonOriginalColor;
    private Color englishButtonOriginalColor;
    private int lastDisplayedScore = int.MinValue;
    private int lastDisplayedSeconds = int.MinValue;
    private bool lastBonusState;

    private void Awake()
    {
        if (spanishButton != null)
        {
            Image image = spanishButton.GetComponent<Image>();
            if (image != null)
            {
                spanishButtonOriginalColor = image.color;
            }
        }

        if (englishButton != null)
        {
            Image image = englishButton.GetComponent<Image>();
            if (image != null)
            {
                englishButtonOriginalColor = image.color;
            }
        }

        ResetSaveFlow();
    }

    private void Start()
    {
        HideLevelCompleteAndWinPanels();
        HideHighScorePanel();
        HideOptionsPanel();
        InitializeOptions();
        UpdateMuteVisual();
        UpdateLanguageButtonsVisual();

        if (RunManager.Instance == null || !RunManager.Instance.RunStarted)
        {
            ShowStartPanel();
        }
        else
        {
            HideStartPanel();
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.Playing)
        {
            ShowHud();
            UpdateHud();
        }
        else
        {
            HideHud();
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            ShowHud();
            UpdateHud();
        }
        else if (GameManager.Instance.CurrentState == GameState.GameOver &&
                 feedbackText != null &&
                 !string.IsNullOrEmpty(feedbackText.text))
        {
            ShowHud();
        }
        else
        {
            HideHud();
        }
    }

    public void ShowStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    public void HideStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
    }

    public void ShowOptionsPanel()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiClick);
        HideStartPanel();

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }

        UpdateMuteVisual();
        UpdateLanguageButtonsVisual();
    }

    public void HideOptionsPanel()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void OnMuteButtonPressed()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.SetMuted(!AudioManager.Instance.IsMuted);
        UpdateMuteVisual();
        AudioManager.Instance.PlaySFX(SfxType.UiClick);
    }

    public void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void OnSfxSliderChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    public void OnSpanishButtonPressed()
    {
        GameSettings.SetLanguage(GameLanguage.Spanish);
        LocalizationManager.RefreshAll();
        UpdateLanguageButtonsVisual();
        AudioManager.Instance?.PlaySFX(SfxType.UiClick);
    }

    public void OnEnglishButtonPressed()
    {
        GameSettings.SetLanguage(GameLanguage.English);
        LocalizationManager.RefreshAll();
        UpdateLanguageButtonsVisual();
        AudioManager.Instance?.PlaySFX(SfxType.UiClick);
    }

    public void OnOptionsReturnButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiReturn);
        HideOptionsPanel();
        ShowStartPanel();
    }

    private void InitializeOptions()
    {
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
            }
        }
    }

    private void UpdateMuteVisual()
    {
        bool isMuted = AudioManager.Instance != null &&
            AudioManager.Instance.IsMuted;

        if (muteImage != null)
        {
            muteImage.SetActive(isMuted);
        }

        if (unmuteImage != null)
        {
            unmuteImage.SetActive(!isMuted);
        }
    }

    private void UpdateLanguageButtonsVisual()
    {
        bool spanishSelected =
            GameSettings.CurrentLanguage == GameLanguage.Spanish;

        SetLanguageButtonColor(
            spanishButton,
            spanishSelected,
            spanishButtonOriginalColor);
        SetLanguageButtonColor(
            englishButton,
            !spanishSelected,
            englishButtonOriginalColor);
    }

    private void SetLanguageButtonColor(
        Button button,
        bool isSelected,
        Color originalColor)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.color = isSelected
            ? new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                1f)
            : new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0.2f);
    }

    public void OnPlayButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiPlay);

        if (GameManager.Instance != null)
        {
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.FadeOutAndBack(StartGameAfterTransition);
            }
            else
            {
                StartGameAfterTransition();
            }
        }
    }

    private void StartGameAfterTransition()
    {
        HideStartPanel();
        GameManager.Instance.StartGame();
    }

    public void ShowHighScorePanel()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiClick);

        if (saveManager == null)
        {
            Debug.LogError(
                "UIManager requires a SaveManager to show high scores.",
                this);
            return;
        }

        HideStartPanel();

        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);
        }

        StringBuilder namesBuilder = new StringBuilder();
        StringBuilder scoresBuilder = new StringBuilder();

        foreach (HighScoreEntry entry in saveManager.GetHighScores())
        {
            namesBuilder.AppendLine(entry.playerName);
            scoresBuilder.AppendLine(entry.score.ToString());
        }

        if (highScoreNamesText != null)
        {
            highScoreNamesText.text = namesBuilder.ToString();
        }

        if (highScoreValuesText != null)
        {
            highScoreValuesText.text = scoresBuilder.ToString();
        }
    }

    public void HideHighScorePanel()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }

    public void OnHighScoreReturnButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiReturn);
        HideHighScorePanel();
        ShowStartPanel();
    }

    private void ShowHud()
    {
        if (hudCanvas != null &&
            !hudCanvas.activeSelf)
        {
            hudCanvas.SetActive(true);
        }
    }

    private void HideHud()
    {
        if (hudCanvas != null &&
            hudCanvas.activeSelf)
        {
            hudCanvas.SetActive(false);
        }
    }

    private void UpdateHud()
    {
        if (RunManager.Instance == null ||
            GameManager.Instance == null ||
            ScoreManager.Instance == null)
        {
            return;
        }

        int currentScore = ScoreManager.Instance.LevelScore;

        if (scoreText != null &&
            currentScore != lastDisplayedScore)
        {
            lastDisplayedScore = currentScore;
            scoreText.text = $"SCORE: {currentScore}";
        }

        if (timerText == null)
        {
            return;
        }

        bool isBonus =
            GameManager.Instance.CurrentLevelIsBonus();

        if (isBonus)
        {
            if (!lastBonusState)
            {
                timerText.text = "BONUS ∞";
                lastBonusState = true;
            }

            return;
        }

        lastBonusState = false;

        int totalSeconds =
            Mathf.CeilToInt(
                GameManager.Instance.TimeRemaining);

        if (totalSeconds == lastDisplayedSeconds)
        {
            return;
        }

        lastDisplayedSeconds = totalSeconds;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }

    public void ShowFeedback(string message)
    {
        if (feedbackText == null)
        {
            return;
        }

        feedbackText.text = message;
        ShowHud();
    }

    public void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }

        lastDisplayedScore = int.MinValue;
        lastDisplayedSeconds = int.MinValue;
        lastBonusState = false;
    }

    public void ShowLevelCompletePanel()
    {
        UpdateLevelCompleteStats();

        if (lvlText != null && RunManager.Instance != null)
        {
            if (ScoreManager.Instance != null &&
                ScoreManager.Instance.LevelScore == 0)
            {
                lvlText.text = LocalizationManager.Get("LEVEL_COMPLETE_ZERO");
            }
            else
            {
                string key = $"LEVEL_COMPLETE_{RunManager.Instance.CurrentLevel}";
                lvlText.text = LocalizationManager.Get(key);
            }
        }

        if (lvlCompletePanel != null)
        {
            lvlCompletePanel.SetActive(true);
            AudioManager.Instance?.PlaySFX(SfxType.LevelComplete);
        }
    }

    private void UpdateLevelCompleteStats()
    {
        if (ScoreManager.Instance == null)
        {
            return;
        }

        if (starsCountText != null)
        {
            starsCountText.text = ScoreManager.Instance.StarsCollected.ToString();
        }

        if (croissantsCountText != null)
        {
            croissantsCountText.text =
                ScoreManager.Instance.CroissantsCollected.ToString();
        }

        if (aliensCountText != null)
        {
            aliensCountText.text = ScoreManager.Instance.AliensKilled.ToString();
        }

        if (levelScoreText != null)
        {
            levelScoreText.text = ScoreManager.Instance.LevelScore.ToString();
        }

        if (lvlScoreLabelText != null && RunManager.Instance != null)
        {
            lvlScoreLabelText.text = string.Format(
                LocalizationManager.Get("LEVEL_SCORE_LABEL"),
                RunManager.Instance.CurrentLevel
            );
        }

        if (totalScoreText != null)
        {
            totalScoreText.text = RunManager.Instance != null
                ? RunManager.Instance.CurrentScore.ToString()
                : "0";
        }
    }

    public void ShowWinPanel()
    {
        ResetSaveFlow();

        if (winText != null && RunManager.Instance != null)
        {
            int score = RunManager.Instance.CurrentScore;

            if (score == 0)
            {
                winText.text = LocalizationManager.Get("WIN_ZERO");
            }
            else if (score >= 4700)
            {
                winText.text = LocalizationManager.Get("WIN_HIGH");
            }
            else
            {
                winText.text = LocalizationManager.Get("WIN_NORMAL");
            }
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = RunManager.Instance != null
                ? RunManager.Instance.CurrentScore.ToString()
                : "0";
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            AudioManager.Instance?.PlaySFX(SfxType.Win);
        }

        if (winLevelScoreText != null && ScoreManager.Instance != null)
        {
            winLevelScoreText.text = $"+{ScoreManager.Instance.LevelScore}";
        }
    }

    public void OnNameButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiClick);

        if (playerNameInput == null || saveButton == null)
        {
            Debug.LogError(
                "UIManager requires PlayerNameInput and SaveButton.",
                this);
            return;
        }

        if (nameButton != null)
        {
            nameButton.gameObject.SetActive(false);
        }

        playerNameInput.characterLimit = SaveManager.MaxPlayerNameLength;
        playerNameInput.text = string.Empty;
        playerNameInput.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
        playerNameInput.Select();
        playerNameInput.ActivateInputField();
    }

    public void OnNameEndEdit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        OnSaveButtonPressed();
    }
    public void OnSaveButtonPressed()
    {
        if (saveManager == null || playerNameInput == null)
        {
            Debug.LogError(
                "UIManager requires SaveManager and PlayerNameInput to save the score.",
                this);
            return;
        }

        if (!saveManager.SaveCurrentRunScore(playerNameInput.text))
        {
            return;
        }

        AudioManager.Instance?.PlaySFX(SfxType.UiPlay);

        if (RunManager.Instance != null)
        {
            RunManager.Instance.EndRun();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToStart();
        }
    }

    public void HideLevelCompleteAndWinPanels()
    {
        if (lvlCompletePanel != null)
        {
            lvlCompletePanel.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        HideHud();
    }

    public void OnNextButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiPlay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNextLevel();
        }
    }

    public void OnReturnButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiReturn);

        if (RunManager.Instance != null)
        {
            RunManager.Instance.EndRun();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToStart();
        }
    }

    private void ResetSaveFlow()
    {
        if (nameButton != null)
        {
            nameButton.gameObject.SetActive(true);
        }

        if (playerNameInput != null)
        {
            playerNameInput.characterLimit = SaveManager.MaxPlayerNameLength;
            playerNameInput.text = string.Empty;
            playerNameInput.gameObject.SetActive(false);
        }

        if (saveButton != null)
        {
            saveButton.gameObject.SetActive(false);
        }
    }

    public void OnQuitButtonPressed()
    {
        AudioManager.Instance?.PlaySFX(SfxType.UiReturn);

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
            }

}