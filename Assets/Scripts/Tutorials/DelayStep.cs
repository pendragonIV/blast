using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/DelayStep")]
public class DelayStep : TutorialStep
{
    [Tooltip("In milliseconds")]
    [SerializeField]
    private int _delay;

    public override void Execute(TutorialManager tutorialManager)
    {
        base.Execute(tutorialManager);
        PerformDelay();
    }

    private async void PerformDelay()
    {
        await UniTask.Delay(_delay);
        EndStep();
    }
}
