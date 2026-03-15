using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityGrid : MonoBehaviour
{
    [SerializeField]
    private bool _isSetupOnStart;
    [SerializeField]
    private bool _isShowPreview;
    [SerializeField]
    private Vector3 _cellSize;
    [SerializeField]
    private Vector3 _cellGap;
    [SerializeField]
    private Vector2 _gridSize;
    [SerializeField]
    private Vector3 _origin;
    [SerializeField]
    private GridVerticalDirection _gridVertDirection;

    protected Dictionary<Vector2, GridNode> _gridNodes = new();

    private void Start() {
        if (!_isSetupOnStart)
        {
            return;
        }

        CreateGrid();
    }

    public void SetCellGap(float gap)
    {
        _cellGap = new Vector3(gap, 0f, gap);
    }

    public void SetupGrid(Vector2 gridSize)
    {
        _gridSize = gridSize;
        CreateGrid();
    }

    public GridNode GetGridNode(Vector2 gridCoordinate)
    {
        if (!_gridNodes.ContainsKey(gridCoordinate))
        {
            Debug.LogError($"Grid node does not exist at {gridCoordinate.x}, {gridCoordinate.y}");
            return null;
        }

        return _gridNodes[gridCoordinate];
    }

    public GridNode GetEmptyGridNode()
    {
        return _gridNodes.Values.FirstOrDefault(node => !node.IsOccupied);
    }

    public List<GridNode> GetAllEmptyGridNodes()
    {
        return _gridNodes.Values.Where(node => !node.IsOccupied).ToList();
    }

    public void ClearGrid()
    {
        _gridNodes.Clear();
    }

    private void CreateGrid()
    {
        CentreGridHorizontallyToOrigin();

        var direction = (int)_gridVertDirection;
        for (int i = 0; i < _gridSize.x; i++)
        {
            for (int j = 0; j < _gridSize.y; j++)
            {
                var pos = transform.position + 
                        new Vector3(j * (_cellSize.z + _cellGap.z),
                            0f,
                            direction * i * (_cellSize.x + _cellGap.x));
                var gridCoord = new Vector2(i, j);

                var gridNode = new GridNode();
                gridNode.WorldPos = pos;
                gridNode.GridCoordinate = gridCoord;
                _gridNodes.Add(gridCoord, gridNode);
                
                if (_isShowPreview)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = $"Cube{i}_{j}";
                    cube.transform.parent = transform;
                    cube.transform.position = pos;
                }
            }
        }
    }

    private void CentreGridHorizontallyToOrigin()
    {
        var offsetX = 0.5f * (_gridSize.y - 1) * (_cellSize.x + _cellGap.x); 
        var newPos = _origin - new Vector3(offsetX, 0f, 0f);
        transform.position = newPos;
    }

    public enum GridVerticalDirection
    {
        Up = 1,
        Down = -1,
    }
}
