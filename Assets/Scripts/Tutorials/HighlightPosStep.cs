using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/HighlightPosStep")]
public class HighlightPosStep : TutorialStep
{
    [SerializeField]
    private GridType _gridType;
    [SerializeField]
    private Vector2 _gridNode;

    public override void Execute(TutorialManager tutorialManager)
    {
        base.Execute(tutorialManager);
        var screenPos = GetHighlightScreenPos();
        tutorialManager.SetHighlightPos(screenPos);
        EndStep();
    }

    private Vector3 GetHighlightScreenPos()
    {
        if (_gridType == GridType.ShooterGrid)
        {
            var shooterGrid = _tutorialManager.GetShooterGrid();
            var gridNode = shooterGrid.GetGridNode(_gridNode);
            return _tutorialManager.WorldToScreenPoint(gridNode.WorldPos);
        }

        if (_gridType == GridType.BlockGrid)
        {
            var blockGrid = _tutorialManager.GetBlockGrid();
            var gridNode = blockGrid.GetGridNode(_gridNode);
            return _tutorialManager.WorldToScreenPoint(gridNode.WorldPos);
        }

        return Vector3.zero;
    }
}
