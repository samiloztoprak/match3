using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LogicLibrary;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Vector2Int _boardSize;
    [SerializeField] private PieceView _piecePrefab;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _stepDuration = 0.2f;
    [SerializeField] private float _swapDuration = 0.15f;
    [SerializeField] private PieceVisualConfig _visualConfig;
    [SerializeField] private ParticleSystem _breakEffectPrefab;
    
    private Dictionary<int, PieceView> _pieceById = new Dictionary<int, PieceView>();

    private bool _hasStart;
    private bool _isResolving;
    private int _startX, _startY;
    private Vector3 _startWorldPos;
    private ObjectPool<PieceView> _pieceViewPool;
    private ObjectPool<ParticleSystem> _breakEffectPool;
    
    private Board _board;
    void Start()
    {
        _board = new Board(_boardSize.x, _boardSize.y);

        Vector3 defaultScale = _piecePrefab.transform.localScale;
        _pieceViewPool = new ObjectPool<PieceView>(
            createFunc: () =>Instantiate(_piecePrefab, transform),
            actionOnGet: view => view.gameObject.SetActive(true),
            actionOnRelease: view =>
            {
                view.gameObject.SetActive(false);
                view.transform.localScale = defaultScale;
            },
            actionOnDestroy: view => Destroy(view.gameObject),
            collectionCheck: true,
            defaultCapacity: _board !=  null ? _board.Width * _board.Height : 25,
            maxSize: 200);

        _breakEffectPool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(_breakEffectPrefab, transform),
            actionOnGet: effect => effect.gameObject.SetActive(true),
            actionOnRelease: effect => effect.gameObject.SetActive(false),
            actionOnDestroy: effect => Destroy(effect.gameObject),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50);
        
        LogicLibrary.OrganizationLogic.GetNewPieceToEmpty(_board);
        _board.SetCell(2, 2, new BoxPiece(_board.NextId(), PieceColor.Empty));
        
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 2f;
        
        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                PieceView instance = _pieceViewPool.Get();
                instance.transform.SetPositionAndRotation(CellToWorld(x, y), Quaternion.identity);
                Piece piece = _board.GetCell(x, y);
                instance.Setup(piece, _visualConfig);
                _pieceById[piece.Id] = instance;
            }
        }

        HashSet<(int x, int y)> overlayCells = new HashSet<(int x, int y)> { (1, 1) };
        LockPiece  lockPiece = new LockPiece (_board.NextId(), overlayCells);
        _board.AddOverlayPiece(lockPiece);
        foreach (var (x, y) in overlayCells)
        {
            Piece covered = _board.GetCell(x, y);
            if (covered != null)
            {
                BoardLogic.AddFlag(covered, PieceFlags.Locked);
            }
            
            PieceView lockView = _pieceViewPool.Get();
            lockView.transform.SetPositionAndRotation(CellToWorld(x, y), Quaternion.identity);
            lockView.Setup(lockPiece, _visualConfig);
            lockView.SetSortingOrder(1);
            _pieceById[lockPiece.Id] = lockView;
        }
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            _startWorldPos = worldPos;
            (_startX, _startY) = WorldToCell(worldPos);
            _hasStart = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && _hasStart)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 endWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            Vector3 delta = endWorldPos - _startWorldPos;

            if(delta.magnitude > .5f)
            {
                int targetX = _startX;
                int targetY = _startY;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    targetX += delta.x > 0 ? 1 : -1;
                else
                    targetY += delta.y > 0 ? -1 : 1;

                TrySwap(_startX, _startY, targetX, targetY);
                _hasStart = false;
            }
        }
    }

    public void TrySwap(int x1, int y1, int x2, int y2)
    {
        if (_isResolving)
        {
            Debug.Log("Still resolving previous turn, ignoring this swap");
            return;
        }

        if (!BoardLogic.IsValidIndex(x1, y1, _board))
        {
            Debug.Log("Invalid index 1");
            return;
        }

        if (!BoardLogic.IsValidIndex(x2, y2, _board))
        {
            Debug.Log("Invalid index 2");
            return;
        }

        Piece piece1 = _board.GetCell(x1, y1);
        Piece piece2 =  _board.GetCell(x2, y2);

        if (piece1 == null || piece2 == null)
        {
            Debug.Log("One or Two pieces is null");
            return;
        }
        
        if (BoardLogic.IsPieceLocked(piece1) || BoardLogic.IsPieceLocked(piece2))
        {
            Debug.Log("One of the pieces is not movable, has locked flag");
            return;
        }
        
        if (!BoardLogic.IsPieceMoveable(piece1) || !BoardLogic.IsPieceMoveable(piece2))
        {
            Debug.Log("One of the pieces is not movable, has not moveable flag");
            return;
        }
        
        var command = new SwapCommand(_board, x1, y1, x2, y2);
        var result = TurnResolver.Resolve(_board, command);

        StartCoroutine(PlayTurn(piece1.Id, piece2.Id, x1, y1, x2, y2, result));
    }

    private IEnumerator PlayTurn(int piece1Id, int piece2Id, int x1, int y1, int x2, int y2, List<TurnStepData> steps)
    {
        _isResolving = true;
        try
        {
            yield return PlayTurnSteps(piece1Id, piece2Id, x1, y1, x2, y2, steps);
        }
        finally
        {
            _isResolving = false;
        }
    }

    private IEnumerator PlayTurnSteps(int piece1Id, int piece2Id, int x1, int y1, int x2, int y2, List<TurnStepData> steps)
    {
        if(_pieceById.TryGetValue(piece1Id, out PieceView view1))
        {
            view1.MoveTo(CellToWorld(x2, y2), _swapDuration);
        }     
        
        if(_pieceById.TryGetValue(piece2Id, out PieceView view2))
        {
            view2.MoveTo(CellToWorld(x1, y1), _swapDuration);
        }        
        yield return new WaitForSeconds(_swapDuration);

        if (steps.Count == 0)
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
        
        foreach (var step in steps)
        {
            if (step is ClearPieceData clearStep)
            {
                foreach (int pieceId in clearStep.PieceIds)
                {
                    if (_pieceById.TryGetValue(pieceId, out PieceView view))
                    {
                        SpawnBreakEffect(view.transform.position);
                        view.ShrinkAndRelease(_stepDuration, v => _pieceViewPool.Release(v));
                        _pieceById.Remove(pieceId);
                    }
                }
                yield return new WaitForSeconds(_stepDuration);
            }
            else if (step is DamagePieceData damagePiece)
            {
                foreach (int pieceId in damagePiece.PieceIds)
                {
                    if (_pieceById.TryGetValue(pieceId, out PieceView view))
                    {
                        SpawnBreakEffect(view.transform.position);
                        view.RefreshSprite(_visualConfig);
                    }
                }
                yield return new WaitForSeconds(_stepDuration);
            }
            else if (step is ClearOverlayData clearOverlayStep)
            {
                foreach (int pieceId in clearOverlayStep.PieceIds)
                {
                    if (_pieceById.TryGetValue(pieceId, out PieceView view))
                    {
                        SpawnBreakEffect(view.transform.position);
                        view.ShrinkAndRelease(_stepDuration, v => _pieceViewPool.Release(v));
                        _pieceById.Remove(pieceId);
                    }
                }
                yield return new WaitForSeconds(_stepDuration);
            }
            else if (step is FallStepData fallStep)
            {
                foreach (var (pieceId, toX, toY) in fallStep.Moves)
                {
                    if (_pieceById.TryGetValue(pieceId, out PieceView view))
                    {
                        view.MoveTo(CellToWorld(toX, toY), _stepDuration, Ease.InQuad, true);
                    }
                }
                yield return new WaitForSeconds(_stepDuration);
            }
            else if (step is RefillStepData refillStep)
            {
                Dictionary<int, int> totalPerColumn = new Dictionary<int, int>();
                foreach (var (pieceId, color, toX, toY) in refillStep.Moves)
                {
                    if (!totalPerColumn.ContainsKey(toX))
                    {
                        totalPerColumn[toX] = 0;
                    }
                    totalPerColumn[toX]++;
                }

                foreach (var (pieceId, color, toX, toY) in refillStep.Moves)
                {
                    Vector3 spawnPos = CellToWorld(toX, toY - totalPerColumn[toX]);
                    PieceView instance = _pieceViewPool.Get();
                    instance.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
                    instance.Setup(new ColoredPiece(pieceId, color), _visualConfig);
                    _pieceById[pieceId] = instance;

                    instance.MoveTo(CellToWorld(toX, toY), _stepDuration, Ease.InQuad, true);
                }
                yield return new WaitForSeconds(_stepDuration);
            }
        }
    }

    private (int, int) WorldToCell(Vector3 worldPos)
    {
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 2f;

        int x = Mathf.RoundToInt(worldPos.x / _cellSize + offsetX);
        int y = Mathf.RoundToInt(offsetY - worldPos.y / _cellSize);
        return (x, y);
    }
    
    private Vector3 CellToWorld(int x, int y)
    {
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 2f;
        return new Vector3((x - offsetX) * _cellSize, -(y - offsetY) * _cellSize, 0);
    }

    private void SpawnBreakEffect(Vector3 position)
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
}
