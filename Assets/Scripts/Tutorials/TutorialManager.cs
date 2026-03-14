using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    private List<LevelTutorial> _levelTutorials;
    [SerializeField]
    private EntityGrid _shooterGrid;
     [SerializeField]
    private EntityGrid _blockGrid;
    [SerializeField]
    private ShooterManager _shooterManager;
    [SerializeField]
    private Camera _camera;

    [Header("Canvas")]
    [SerializeField]
    private Canvas _tutCanvas;

    [Header("Pointer")]
    [SerializeField]
    private RectTransform _pointer;
    [SerializeField]
    private RectTransform _pointerImage;
    [SerializeField]
    private TextMeshProUGUI _pointerInstruction;

    [Header("Highlight")]
    [SerializeField]
    private RectTransform _greyTint;
    [SerializeField]
    private RectTransform _highlight;

    [Header("Pop-up message")]
    [SerializeField]
    private GameObject _popUpPanel;
    [SerializeField]
    private TextMeshProUGUI _mainMessage;
    [SerializeField]
    private TextMeshProUGUI _subMessage;
    [SerializeField]
    private Button _popUpCloseButton;
    [SerializeField]
    private AudioClip _buttonSfx;
    [SerializeField]
    private GameObject _pureTint;

    private Dictionary<int, LevelTutorial> _levelTutorialMap = new();

    private void Awake() 
    {
        for (int i = 0; i < _levelTutorials.Count; i++)
        {
            _levelTutorialMap.Add(_levelTutorials[i].Level, _levelTutorials[i]);
        }

        _gameManager.OnLevelStarted += OnLevelStarted;
        _popUpCloseButton.onClick.AddListener(OnPopUpClose);
        HidePopUpMessage();
    }

    private void OnDestroy() {
        _gameManager.OnLevelStarted -= OnLevelStarted;
        _popUpCloseButton.onClick.RemoveAllListeners();
    }

    public void StartTutorial()
    {
        _tutCanvas.enabled = true;
    }

    public void EndTutorial()
    {
        _tutCanvas.enabled = false;
        _highlight.gameObject.SetActive(false);
        _pointer.gameObject.SetActive(false);
    }

    public EntityGrid GetShooterGrid()
    {
        return _shooterGrid;
    }

    public EntityGrid GetBlockGrid()
    {
        return _blockGrid;
    }

    public Shooter GetShooter(Vector2 gridCoordinate)
    {
        return _shooterManager.GetShooter(gridCoordinate);
    }

    public Vector3 WorldToScreenPoint(Vector3 worldPos)
    {
        return _camera.WorldToScreenPoint(worldPos);
    }

    public void SetPointerPos(Vector3 screenPos, bool isShowInstruction = true)
    {
        _pointer.transform.position = screenPos;

        var seq = DOTween.Sequence();

        seq.AppendInterval(1f)
        .Append(_pointerImage.DOLocalRotate(new Vector3(0f, 0f, 15f), 0.5f).SetEase(Ease.InOutSine))
        .SetLoops(-1);
        _pointer.gameObject.SetActive(true);
        _pointerInstruction.gameObject.SetActive(isShowInstruction);
    }

    public void SetHighlightPos(Vector3 screenPos)
    {
        _highlight.transform.position = screenPos;
        _greyTint.gameObject.SetActive(true);
        _greyTint.transform.position = new Vector2(Screen.width/2f, Screen.height/2f);
        _highlight.gameObject.SetActive(true);
    }

    public void ShowPopUpMessage(string mainMessage, string subMessage, bool isShowCloseButton, bool isAtBottom, bool isShowPureTint = false)
    {
        _mainMessage.SetText(mainMessage);
        _subMessage.SetText(subMessage);

        _popUpCloseButton.gameObject.SetActive(isShowCloseButton);
        _subMessage.gameObject.SetActive(!isShowCloseButton);
        _pureTint.SetActive(isShowPureTint);

        if (isAtBottom)
        {
            _popUpPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -700f, 0f);
        }
        else
        {
            _popUpPanel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
        _popUpPanel.SetActive(true);
    }

    public void HidePopUpMessage()
    {
        _popUpPanel.SetActive(false);
        _pureTint.SetActive(false);
    }

    private void OnLevelStarted()
    {
        if (_levelTutorialMap.ContainsKey(_gameManager.CurrentLevel))
        {
            StartTutorial();
            _levelTutorialMap[_gameManager.CurrentLevel].Begin(this);
        }
    }

    private void OnPopUpClose()
    {
        AudioManager.Instance.PlayOneShot(_buttonSfx);
        HidePopUpMessage();
        EndTutorial();
    }
}
