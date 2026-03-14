using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/ShowPopUpMessageStep")]
public class ShowPopUpMessageStep : TutorialStep
{
    [SerializeField]
    private string _mainMessage;
    [SerializeField]
    private string _subMessage;
    [SerializeField]
    private bool _isShowCloseButton;
    [SerializeField]
    private bool _isAtBottom = false;
    [SerializeField]
    private bool _isShowPureTint = false;

    public override void Execute(TutorialManager tutorialManager)
    {
        base.Execute(tutorialManager);
        tutorialManager.ShowPopUpMessage(_mainMessage, _subMessage, _isShowCloseButton, _isAtBottom, _isShowPureTint);

        if (!_isShowCloseButton)
            EndStep();
    }
}
