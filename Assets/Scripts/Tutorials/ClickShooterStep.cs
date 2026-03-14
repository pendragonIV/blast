using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/ClickShooterStep")]
public class ClickShooterStep : TutorialStep
{
    [SerializeField]
    private Vector2 _gridCoordinate;

    public override void Execute(TutorialManager tutorialManager)
    {
        base.Execute(tutorialManager);
        var shooter = tutorialManager.GetShooter(_gridCoordinate);
        shooter.OnClick += OnShooterClicked;
    }

    private void OnShooterClicked(Shooter shooter)
    {
        shooter.OnClick -= OnShooterClicked;
        EndStep();
    }
}
