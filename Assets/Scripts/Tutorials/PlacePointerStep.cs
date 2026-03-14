using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/PlacePointerStep")]
public class PlacePointerStep : TutorialStep
{
    [SerializeField]
    private GridType _gridType;
    [SerializeField]
    private Vector2 _gridNode;
    [SerializeField]
    private bool _isShowInstruction;


    public override void Execute(TutorialManager tutorialManager)
    {
        base.Execute(tutorialManager);
        var screenPos = GetPointerScreenPos();
        _tutorialManager.SetPointerPos(screenPos, _isShowInstruction);
        EndStep();
    }

    private Vector3 GetPointerScreenPos()
    {
        if (_gridType == GridType.ShooterGrid)
        {
            var shooterGrid = _tutorialManager.GetShooterGrid();
            var gridNode = shooterGrid.GetGridNode(_gridNode);
            return _tutorialManager.WorldToScreenPoint(gridNode.WorldPos);
        }

        return Vector3.zero;
    }

}
