
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
    [SerializeField] private float _fallDuration = 0.2f;
    [SerializeField] private float _fallDelay = 0.06f;
    [SerializeField] private float _mergeDuration = 0.16f;
    [SerializeField] private PieceVisualConfig _visualConfig;
    [SerializeField] private ParticleSystem _breakEffectPrefab;
    [SerializeField] private PropellerAnimationConfig _propellerConfig;
    public PropellerAnimationConfig PropellerConfig => _propellerConfig;
    public float PopDuration => _stepDuration;
    public float FallDuration => _fallDuration;
    public float FallDelay => _fallDelay;
    public float MergeDuration => _mergeDuration;
    
    
    private Dictionary<int, PieceView> _pieceById = new Dictionary<int, PieceView>();
    private ObjectPool<PieceView> _pieceViewPool;
    private ObjectPool<ParticleSystem> _breakEffectPool;
    private Board _board;
    
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
                view.transform.DOKill();
                view.gameObject.SetActive(false);
                view.transform.localScale = defaultScale;
                view.SetSortingOrder(SortingOrders.Piece);
            },
            actionOnDestroy: view => Destroy(view.gameObject),
            collectionCheck: true,
            defaultCapacity: board.Width * board.Height,
            maxSize: 200);

        _breakEffectPool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(_breakEffectPrefab, transform),
            actionOnGet: effect => effect.gameObject.SetActive(true),
            actionOnRelease: effect => effect.gameObject.SetActive(false),
            actionOnDestroy: effect => Destroy(effect.gameObject),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50);
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
        float offsetY = (_board.Height - 1) / 2f;
        return new Vector3((x - offsetX) * _cellSize, -(y - offsetY) * _cellSize, 0);
    }
    
    public (int, int) WorldToCell(Vector3 worldPos)
    {
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 2f;
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
    
    public void SpawnBreakEffect(Vector3 position)
    {
        ParticleSystem effect = _breakEffectPool.Get();
        effect.transform.position = position;
        effect.Play();
        StartCoroutine(ReleaseBreakAfterPlay(effect));
    }
    
    private IEnumerator ReleaseBreakAfterPlay(ParticleSystem effect)
    {
        yield return new WaitForSeconds(effect.main.duration + effect.main.startLifetime.constantMax);
        _breakEffectPool.Release(effect);
    }
    
    public IEnumerator PlaySwapTurn(int piece1Id, int piece2Id, int x1, int y1, int x2, int y2, List<TurnStepData> steps)
    {
        IsBusy = true;
        try
        {
            if (_pieceById.TryGetValue(piece1Id, out PieceView view1))
            {
                view1.MoveTo(CellToWorld(x2, y2), _swapDuration);
            }
            if (_pieceById.TryGetValue(piece2Id, out PieceView view2))
            {
                view2.MoveTo(CellToWorld(x1, y1), _swapDuration);
            }
            yield return new WaitForSeconds(_swapDuration);

            List<(float startTime, IViewCommand command)> commands = ViewCommandFactory.Build(steps, this);
            
            if (commands.Count == 0)
            {
                if (_pieceById.TryGetValue(piece1Id, out PieceView backView1))
                {
                    backView1.MoveTo(CellToWorld(x1, y1), _swapDuration);
                }
                if (_pieceById.TryGetValue(piece2Id, out PieceView backView2))
                {
                    backView2.MoveTo(CellToWorld(x2, y2), _swapDuration);
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
