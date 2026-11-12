
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

public class TurnPlayer : MonoBehaviour
{
    [SerializeField] private PieceView _piecePrefab;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _stepDuration = 0.2f;  
    [SerializeField] private float _swapDuration = 0.15f;
    [SerializeField] private float _fallSecondsPerCell = 0.08f;
    [SerializeField] private float _fallDelay = 0.06f;
    [SerializeField] private float _mergeDuration = 0.16f;
    [SerializeField] private PieceVisualConfig _visualConfig;
    [SerializeField] private BreakEffectConfig _breakEffects;
    [SerializeField] private PropellerAnimationConfig _propellerConfig;
    [SerializeField] private RocketActivationView _rocketActivationPrefab;
    [SerializeField] private EffectSpawner _effects;
    [SerializeField] private CameraShake _cameraShake;
    [SerializeField] private ExplosionView _tntExplosionPrefab;
    [SerializeField] private LightBeamView _lightBeamPrefab;
    [SerializeField] private PropellerActivationView _propellerActivationPrefab;
    
    public PropellerAnimationConfig PropellerConfig => _propellerConfig;
    public EffectSpawner Effects => _effects;
    public CameraShake CameraShake => _cameraShake;
    public ExplosionView TntExplosionPrefab => _tntExplosionPrefab;
    
    public float PopDuration => _stepDuration;
    public float FallSecondsPerCell => _fallSecondsPerCell;
    public float FallDelay => _fallDelay;
    public float MergeDuration => _mergeDuration;


    
    private Dictionary<int, PieceView> _pieceById = new Dictionary<int, PieceView>();
    private ObjectPool<PieceView> _pieceViewPool;
    private ObjectPool<ParticleSystem> _breakEffectPool;
    private Board _board;
    
    public int BoardWidth => _board.Width;
    public int BoardHeight => _board.Height;
    public float CellSize => _cellSize;
    
    public bool IsBusy { get; private set; }
    public PieceVisualConfig VisualConfig => _visualConfig;

    public void Init(Board board)
    {
        _board = board;

        Vector3 defaultScale = _piecePrefab.transform.localScale;
        _pieceViewPool = new ObjectPool<PieceView>(
            createFunc: () => Instantiate(_piecePrefab, transform),
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
        instance.Setup(piece, _visualConfig);
        _pieceById[piece.Id] = instance;
        return instance;
    }
    
    public RocketActivationView SpawnRocketActivation(Vector3 position)
    {
        return _effects.Spawn(_rocketActivationPrefab, position);
    }

    public void ReleaseRocketActivation(RocketActivationView view)
    {
        _effects.Release(view);
    }
    
    public LightBeamView SpawnLightBeam(Vector3 position)
    {
        return _effects.Spawn(_lightBeamPrefab, position);
    }

    public void ReleaseLightBeam(LightBeamView view)
    {
        _effects.Release(view);
    }
    
    public PropellerActivationView SpawnPropellerActivation(Vector3 position)
    {
        return _effects.Spawn(_propellerActivationPrefab, position);
    }

    public void ReleasePropellerActivation(PropellerActivationView view)
    {
        _effects.Release(view);
    }
    
    public void SpawnBreakEffect(Vector3 position)
    {
        if (_breakEffects == null)
        {
            return;
        }
        PlayEffect(_breakEffects.Default, position);
    }

    public void SpawnBreakEffect(Piece piece, Vector3 position)
    {
        if (_breakEffects == null)
        {
            return;
        }
        PlayEffect(_breakEffects.GetBreak(piece), position);
    }

    public void SpawnHitEffect(Piece piece, Vector3 position)
    {
        if (_breakEffects == null)
        {
            return;
        }
        PlayEffect(_breakEffects.GetHit(piece), position);
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
                view1.MoveTo(targetWorldPos, _swapDuration);
            }
            if (_pieceById.TryGetValue(piece2Id, out PieceView view2))
            {
                view2.MoveTo(isCombo ? targetWorldPos : sourceWorldPos, _swapDuration);
            }
            
            if (!isCombo)
            {
                yield return new WaitForSeconds(_swapDuration);
            }

            List<(float startTime, IViewCommand command)> commands = ViewCommandFactory.Build(steps, this);

            if (commands.Count == 0)
            {
                if (_pieceById.TryGetValue(piece1Id, out PieceView backView1))
                {
                    backView1.MoveTo(sourceWorldPos, _swapDuration);
                }
                if (_pieceById.TryGetValue(piece2Id, out PieceView backView2))
                {
                    backView2.MoveTo(targetWorldPos, _swapDuration);
                }
                yield return new WaitForSeconds(_swapDuration);
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
}
