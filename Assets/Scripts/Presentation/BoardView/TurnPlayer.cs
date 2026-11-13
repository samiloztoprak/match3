
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

public class TurnPlayer : MonoBehaviour
{
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private GameAssets _assets;
    [SerializeField] private AnimationSettings _anim;
    [SerializeField] private EffectSpawner _effects;
    [SerializeField] private CameraShake _cameraShake;
    [SerializeField] private GoalFlyer _goalFlyer;
    [SerializeField] private SpriteRenderer _targetReticle;
    
    public PropellerAnimationConfig PropellerConfig => _assets.PropellerAnimation;
    public ExplosionView TntExplosionPrefab => _assets.TntExplosion;
    public SpriteRenderer HammerPrefab => _assets.Hammer;
    public SpriteRenderer HammerImpactPrefab => _assets.HammerImpact;
    public float PopDuration => _anim.PopDuration;
    public float FallSecondsPerCell => _anim.FallSecondsPerCell;
    public float FallDelay => _anim.FallDelay;
    public float MergeDuration => _anim.MergeDuration;
    public float CollectJumpDuration => _anim.CollectJumpDuration;
    public float CollectFlyDuration => _anim.CollectFlyDuration;
    public PieceVisualConfig VisualConfig => _assets.PieceVisuals;
    public GoalFlyer GoalFlyer => _goalFlyer;
    public CameraShake CameraShake => _cameraShake;
    public EffectSpawner Effects => _effects;
    
    private Dictionary<int, PieceView> _pieceById = new Dictionary<int, PieceView>();
    private ObjectPool<PieceView> _pieceViewPool;
    private Board _board;
    
    public int BoardWidth => _board.Width;
    public int BoardHeight => _board.Height;
    public float CellSize => _cellSize;
    
    public bool IsBusy { get; private set; }
    

    public void Init(Board board)
    {
        _board = board;
        
        Vector3 defaultScale = _assets.PiecePrefab.transform.localScale;
        _pieceViewPool = new ObjectPool<PieceView>(
            createFunc: () => Instantiate(_assets.PiecePrefab, transform),
            actionOnGet: view => view.gameObject.SetActive(true),
            actionOnRelease: view =>
            {
                view.KillTweens();
                view.gameObject.SetActive(false);
                view.transform.localScale = defaultScale;
                view.transform.rotation = Quaternion.identity;
                view.SetSortingOrder(SortingOrders.Piece);
            },
            actionOnDestroy: view => Destroy(view.gameObject),
            collectionCheck: true,
            defaultCapacity: board.Width * board.Height,
            maxSize: 200);
    }
    
    private void OnValidate()
    {
        if (_effects == null) _effects = GetComponent<EffectSpawner>();
        if (_goalFlyer == null) _goalFlyer = GetComponent<GoalFlyer>();
        if (_cameraShake == null) _cameraShake = AutoRef.InScene<CameraShake>();
        if (_targetReticle == null) _targetReticle = AutoRef.Child<SpriteRenderer>(this, "fx_target_reticle");
#if UNITY_EDITOR
        if (_assets == null) _assets = AutoRef.Asset<GameAssets>();
        if (_anim == null) _anim = AutoRef.Asset<AnimationSettings>();
#endif
    }
    
    public PieceView GetView(int pieceId)
    {
        return _pieceById.TryGetValue(pieceId, out var v) ? v : null;
    }

    public void RegisterView(int pieceId, PieceView view)
    {
        _pieceById[pieceId] = view;
    }

    public void UnregisterView(int pieceId)
    {
        _pieceById.Remove(pieceId);
    }
    
    public void ReleasePooled(PieceView view)
    {
        _pieceViewPool.Release(view);
    }
    
    public Vector3 CellToWorld(int x, int y)
    {
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 3f;
        return new Vector3((x - offsetX) * _cellSize, -(y - offsetY) * _cellSize, 0);
    }
    
    public (int, int) WorldToCell(Vector3 worldPos)
    {
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 3f;
        int x = Mathf.RoundToInt(worldPos.x / _cellSize + offsetX);
        int y = Mathf.RoundToInt(offsetY - worldPos.y / _cellSize);
        return (x, y);
    }
    
    public PieceView SpawnPieceView(Piece piece, Vector3 position)
    {
        PieceView instance = _pieceViewPool.Get();
        instance.transform.SetPositionAndRotation(position, Quaternion.identity);
        instance.Setup(piece, _assets.PieceVisuals);
        _pieceById[piece.Id] = instance;
        return instance;
    }
    
    public RocketActivationView SpawnRocketActivation(Vector3 position)
    {
        return _effects.Spawn(_assets.RocketActivation, position);
    }

    public void ReleaseRocketActivation(RocketActivationView view)
    {
        _effects.Release(view);
    }
    
    public LightBeamView SpawnLightBeam(Vector3 position)
    {
        return _effects.Spawn(_assets.LightBeam, position);
    }

    public void ReleaseLightBeam(LightBeamView view)
    {
        _effects.Release(view);
    }
    
    public PropellerActivationView SpawnPropellerActivation(Vector3 position)
    {
        return _effects.Spawn(_assets.PropellerActivation, position);
    }

    public void ReleasePropellerActivation(PropellerActivationView view)
    {
        _effects.Release(view);
    }
    
    public void SpawnBreakEffect(Vector3 position)
    {
        if (_assets.BreakEffects == null)
        {
            return;
        }
        PlayEffect(_assets.BreakEffects.Default, position);
    }

    public void SpawnBreakEffect(Piece piece, Vector3 position)
    {
        if (_assets.BreakEffects == null)
        {
            return;
        }
        PlayEffect(_assets.BreakEffects.GetBreak(piece), position);
    }

    public void SpawnHitEffect(Piece piece, Vector3 position)
    {
        if (_assets.BreakEffects == null)
        {
            return;
        }
        PlayEffect(_assets.BreakEffects.GetHit(piece), position);
    }

    private void PlayEffect(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null || _effects == null)
        {
            return;
        }
        
        ParticleSystem effect = _effects.Spawn(prefab, position);
        effect.Clear(true);
        effect.Play(true);
        StartCoroutine(ReleaseEffectAfter(effect, effect.main.duration + effect.main.startLifetime.constantMax));
    }

    private IEnumerator ReleaseEffectAfter(ParticleSystem effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        _effects.Release(effect);
    }
    
    public IEnumerator PlaySwapTurn(int piece1Id, int piece2Id, int x1, int y1, int x2, int y2, List<TurnStepData> steps, bool isCombo = false)
    {
        IsBusy = true;
        try
        {
            Vector3 targetWorldPos = CellToWorld(x2, y2);
            Vector3 sourceWorldPos = CellToWorld(x1, y1);

            if (_pieceById.TryGetValue(piece1Id, out PieceView view1))
            {
                view1.MoveTo(targetWorldPos, _anim.SwapDuration);
            }
            if (_pieceById.TryGetValue(piece2Id, out PieceView view2))
            {
                view2.MoveTo(isCombo ? targetWorldPos : sourceWorldPos, _anim.SwapDuration);
            }
            
            if (!isCombo)
            {
                yield return new WaitForSeconds(_anim.SwapDuration);
            }

            List<(float startTime, IViewCommand command)> commands = ViewCommandFactory.Build(steps, this);

            if (commands.Count == 0)
            {
                if (_pieceById.TryGetValue(piece1Id, out PieceView backView1))
                {
                    backView1.MoveTo(sourceWorldPos, _anim.SwapDuration);
                }
                if (_pieceById.TryGetValue(piece2Id, out PieceView backView2))
                {
                    backView2.MoveTo(targetWorldPos, _anim.SwapDuration);
                }
                yield return new WaitForSeconds(_anim.SwapDuration);
                yield break;
            }

            yield return PlayCommands(commands);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    public IEnumerator PlayTapTurn(List<TurnStepData> steps)
    {
        IsBusy = true;
        try
        {
            yield return PlayCommands(ViewCommandFactory.Build(steps, this));
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    public IEnumerator PlayShuffleTurn(List<TurnStepData> steps)
    {
        IsBusy = true;
        try
        {
            yield return PlayCommands(ViewCommandFactory.Build(steps, this));
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private IEnumerator PlayCommands(List<(float startTime, IViewCommand command)> commands)
    {
        if (commands.Count == 0)
        {
            yield break;
        }

        Sequence sequence = DOTween.Sequence();
        float totalDuration = 0f;
        foreach (var (startTime, command) in commands)
        {
            IViewCommand c = command;
            sequence.InsertCallback(startTime, () => c.Execute());
            totalDuration = Mathf.Max(totalDuration, startTime + c.Duration);
        }
        sequence.InsertCallback(totalDuration, () => { });

        yield return sequence.WaitForCompletion();
    }
    
    public void ShowTargetReticle(int x, int y)
    {
        if (_targetReticle == null) return;
        Transform t = _targetReticle.transform;
        t.DOKill();
        t.position = CellToWorld(x, y);
        t.localScale = Vector3.one * 1.25f;
        _targetReticle.sortingOrder = SortingOrders.Flight;
        _targetReticle.gameObject.SetActive(true);
        t.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
    }

    public void HideTargetReticle()
    {
        if (_targetReticle == null) return;
        _targetReticle.transform.DOKill();
        _targetReticle.gameObject.SetActive(false);
    }
}
