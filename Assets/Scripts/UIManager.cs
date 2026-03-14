using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region HUD
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    private TMP_Text _levelLabel;
    [SerializeField]
    private Slider _levelProgressSlider;
    #endregion

    #region Results Screen
    [SerializeField]
    private GameObject _resultsScreen;
    [SerializeField]
    private TMP_Text _resultsTitleLabel;
    [SerializeField]
    private TMP_Text _resultsLevelLabel;
    [SerializeField]
    private Button _continueButton;
    [SerializeField]
    private AudioClip _victorySfx;
    [SerializeField]
    private AudioClip _buttonSfx;
    #endregion

    private void Awake() 
    {
        _gameManager.OnLevelStarted += OnLevelStarted;
        _gameManager.OnLevelProgressUpdated += OnLevelProgressUpdated;
        _gameManager.OnLevelEnded += OnLevelEnded;
        _continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void OnDestroy() 
    {
        _gameManager.OnLevelStarted -= OnLevelStarted;
        _gameManager.OnLevelProgressUpdated -= OnLevelProgressUpdated;
        _gameManager.OnLevelEnded -= OnLevelEnded;
        _continueButton.onClick.RemoveAllListeners();
    }

    private void OnLevelStarted()
    {
        _levelLabel.SetText($"Level {_gameManager.CurrentLevel}");
        _levelProgressSlider.value = 0f;
    }

    private void OnLevelProgressUpdated()
    {
        _levelProgressSlider.value = _gameManager.CurrentLevelProgress;
    }

    private void OnLevelEnded(int result)
    {
        switch(result)
        {
            case 1:
                ShowVictoryScreen();
                break;
        }
    }

    private void ShowVictoryScreen()
    {
        AudioManager.Instance.PlayOneShot(_victorySfx);
        _resultsTitleLabel.SetText("Victory!");
        _resultsLevelLabel.SetText($"Level {_gameManager.CurrentLevel}");
        _resultsScreen.SetActive(true);
    }

    private void OnContinueButtonClicked()
    {
        AudioManager.Instance.PlayOneShot(_buttonSfx);
        // Go to next level
        _gameManager.GoToNextLevel();
        _resultsScreen.SetActive(false);
    }
}
