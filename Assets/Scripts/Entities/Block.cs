using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Block : Entity
{
    public event Action<Block> OnDie;
    public List<GridNode> GridNodes;
    public Color Colour;
    public bool IsAllColours { get; private set; }
    
    [SerializeField]
    private GameObject _3dModel;
    [SerializeField]
    private int _defaultHealth = 1; // per layer
    [SerializeField]
    private int _numOfLayers = 1;
    [SerializeField]
    private AudioClip _deathSfx;
    private List<GameObject> _blockModels = new();
    private int _currentHealth;
    private GameObject _modelToRemove;

    public void Setup(List<GridNode> gridNodes, Color blockColour, int numOfLayers = 1, int cellSize = 1)
    {
        GridNodes = gridNodes;
        if (blockColour == Color.black)
        {
            IsAllColours = true;
        }
        else
            Colour = blockColour;

        _numOfLayers = numOfLayers;
        gameObject.name = $"Block {gridNodes[0].GridCoordinate.x},{gridNodes[0].GridCoordinate.y}";
        transform.position = GetBlockWorldPos(GridNodes);
        _currentHealth = _defaultHealth * numOfLayers;

        for (int i = 0; i < _numOfLayers; i++)
        {
            var pos = transform.position + new Vector3(0f, i * cellSize, 0f);
            var model = Instantiate(_3dModel, pos, Quaternion.identity, transform);

            if (!IsAllColours)
                SetRendererColour(model.GetComponentInChildren<Renderer>(), Colour);
            _blockModels.Add(model);
        }
    }

    public void DoDamage()
    {
        _currentHealth--;
        var diff = _defaultHealth * _blockModels.Count - _currentHealth;
        if (_currentHealth > 0 && diff == _defaultHealth)
        {
            _modelToRemove = _blockModels[_blockModels.Count - 1];
            _blockModels.Remove(_modelToRemove);
            return;
        }

        if (_currentHealth <= 0)
        {
            IsAlive = false;
        }
    }

    public Vector3 GetTargetPos()
    {
        return _blockModels[_blockModels.Count - 1].transform.position;
    }

    public async void ResolveHit()
    {
        AudioManager.Instance.PlayOneShot(_deathSfx);
        if (_modelToRemove != null)
        {
            var model = _modelToRemove;
            _modelToRemove = null;
            await PlayDeathAnimation(model.transform);
            Destroy(model);
            return;
        }

        if (!IsAlive)
        {
            await PlayDeathAnimation(_blockModels[0].transform);
            OnDie?.Invoke(this);
        }
    }

    public async Task MoveAndUpdateGridNodes(List<GridNode> gridNodes)
    {
        await transform.DOMove(GetBlockWorldPos(gridNodes), 0.4f).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
        GridNodes = gridNodes;
    }

    private Vector3 GetBlockWorldPos(List<GridNode> gridNodes)
    {
        Vector3 sum = Vector3.zero;

        for (int i = 0; i < gridNodes.Count; i++)
        {
            sum += gridNodes[i].WorldPos;
        }

        return sum / gridNodes.Count;
    }

    private async UniTask PlayDeathAnimation(Transform transform)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOPunchScale(Vector3.one * 0.3f, 0.1f, 10, 0.5f));
        seq.Append(transform
            .DOScale(0f, 0.15f)
            .SetEase(Ease.InBack));
        await seq.AsyncWaitForCompletion();
    }
}
