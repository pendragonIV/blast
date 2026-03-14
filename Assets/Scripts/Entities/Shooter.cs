using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HighlightPlus;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class Shooter : Entity
{
    public bool IsMerged = false;
    public bool IsUninteractable;
    public event Action<Shooter> OnClick;
    public event Action<Shooter> OnAmmoConsumed;
    public GridNode GridNode { get; private set; }
    public Color Colour;
    public int CurrentAmmo => _numOfAmmo;
    public bool IsHidden { get; private set; } = false;
    public bool HasConnection => Connection != null;
    public ShooterConnection Connection { get; set; }

    [SerializeField]
    private GameObject _3dModel;
    [SerializeField]
    private int _numOfAmmo = 20;
    [SerializeField]
    private float _shootDelay = 1f;
    [SerializeField]
    private AudioClip _clickSfx;
    [SerializeField]
    private TMP_Text _ammoLabel;
    [SerializeField]
    private Texture2D _hideTexture;
    private bool _canAttack = false;
    private Func<List<Block>> _getShootableBlocksFunc;
    private float _shooterTimer;
    private Renderer _renderer;
    private HighlightEffect _highlightEffect;
    private bool _isClickable;

    private IObjectPool<Ammo> _ammoPool;
    private IObjectPool<ParticleSystemVFX> _hitVFXPool;

    private void Update()
    {
        if (!_canAttack || !IsAlive)
            return;

        if (_shooterTimer > 0f)
        {
            _shooterTimer -= Time.deltaTime;
            return;
        }

        var shootableBlocks = _getShootableBlocksFunc?.Invoke();
        AttackBlock(shootableBlocks);
        _shooterTimer = _shootDelay;
    }

    public void Setup(GridNode gridNode, Color shooterColour, IObjectPool<Ammo> ammoPool, IObjectPool<ParticleSystemVFX> hitVFXPool, int totalAmmo = -1)
    {
        _ammoPool = ammoPool;
        _hitVFXPool = hitVFXPool;
        MoveToGridNode(gridNode, false);
        Colour = shooterColour;
        gameObject.name = $"Shooter {gridNode.GridCoordinate.x},{gridNode.GridCoordinate.y}";
        transform.position = gridNode.WorldPos;

        if (totalAmmo > -1)
        {
            SetAmmoCount(totalAmmo);
        }

        var model = Instantiate(_3dModel, transform);
        _renderer = model.GetComponentInChildren<Renderer>();
        _highlightEffect = _renderer.GetComponent<HighlightEffect>();
        SetRendererColour(_renderer, Colour);
    }

    public async UniTask MoveToGridNode(GridNode gridNode, bool isPlayAnim = true)
    {
        // Unoccupy old grid node
        if (GridNode != null)
        {
            GridNode.IsOccupied = false;
        }

        if (gridNode != null)
        {
            gridNode.IsOccupied = true;
        }

        if (isPlayAnim)
        {
            await transform.DOMove(gridNode.WorldPos, 0.4f).AsyncWaitForCompletion();
        }

        GridNode = gridNode;
    }

    public void SetIsClickable(bool isClickable)
    {
        _isClickable = isClickable;
        _highlightEffect.highlighted = isClickable;
    }

    public void InitiateAttack(Func<List<Block>> getShootableBlocksFunc)
    {
        _canAttack = true;
        _getShootableBlocksFunc = getShootableBlocksFunc;
    }

    public void StopAttack()
    {
        _canAttack = false;
    }

    public async UniTask PlayDeathAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOPunchScale(Vector3.one * 0.3f, 0.1f, 10, 0.5f));
        seq.Append(transform
            .DOScale(0f, 0.15f)
            .SetEase(Ease.InBack));
        await seq.AsyncWaitForCompletion();
    }

    public void HideAmmoCount()
    {
        _ammoLabel.gameObject.SetActive(false);
    }

    public void ShowAmmoCount()
    {
        _ammoLabel.gameObject.SetActive(true);
    }

    [ContextMenu("Hide Shooter")]
    public void HideShooter()
    {
        if (IsHidden)
            return;

        IsHidden = true;
        SetRendererTexture(_renderer, _hideTexture);
        SetRendererColour(_renderer, Color.white);
        HideAmmoCount();
    }

    [ContextMenu("Reveal Shooter")]
    public void RevealShooter()
    {
        if (!IsHidden)
            return;

        IsHidden = false;
        SetRendererTexture(_renderer, null);
        SetRendererColour(_renderer, Colour);
        ShowAmmoCount();
    }

    private async Task AttackBlock(List<Block> shootableBlocks)
    {
        for (int i = 0; i < shootableBlocks.Count; i++)
        {
            var block = shootableBlocks[i];
            if (block.IsAlive && (block.Colour == Colour || block.IsAllColours))
            {
                if (!block.IsAllColours)
                    SetAmmoCount(--_numOfAmmo);
                    
                var targetPos = block.GetTargetPos();
                block.DoDamage();

                await LookAtTarget(targetPos);
                PlayProjectileAnimation(targetPos, block.ResolveHit);

                if (_numOfAmmo <= 0)
                {
                    OnAmmoConsumed?.Invoke(this);
                    IsAlive = false;
                    _canAttack = false;
                }
                return;
            }
        }
    }

    private void SetAmmoCount(int count)
    {
        _numOfAmmo = count;
        _ammoLabel.SetText(_numOfAmmo.ToString());
    }

    private async UniTask LookAtTarget(Vector3 targetPos)
    {
        var lookAtPos = new Vector3(targetPos.x, 0f, targetPos.z);
        var relativeLookAtPos = (lookAtPos - transform.position).normalized;
        if (transform.forward == relativeLookAtPos)
        {
            return;
        }
        await PlayLookAtAnimation(lookAtPos);
    }

    private async UniTask PlayLookAtAnimation(Vector3 lookAtPos)
    {
        await transform.DOLookAt(lookAtPos, 0.05f).AsyncWaitForCompletion();
    }

    private async void PlayProjectileAnimation(Vector3 targetPos, Action onComplete)
    {
        var targetToShooter = transform.position - targetPos;
        targetPos = targetPos + 0.1f * targetToShooter;
        var ammo = _ammoPool.Get();
        ammo.transform.position = transform.position;
        ammo.gameObject.SetActive(true);

        await ammo.transform.DOMove(targetPos, 0.15f).AsyncWaitForCompletion();
        onComplete?.Invoke();
        _ammoPool.Release(ammo);

        var hit = _hitVFXPool.Get();
        hit.transform.position = targetPos;
    }

    private void OnMouseDown()
    {
        if (!IsAlive || IsUninteractable || !_isClickable)
            return;

        OnClick?.Invoke(this);
        AudioManager.Instance.PlayOneShot(_clickSfx);
    }
}
