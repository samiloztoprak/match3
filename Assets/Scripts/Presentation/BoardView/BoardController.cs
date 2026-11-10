using System.Collections;
using System.Collections.Generic;
using LogicLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardController : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f)] private float _swipeThresholdCells = 0.35f;

    [SerializeField] private LevelDefinition[] _levels;
    [SerializeField] private TurnPlayer _turnPlayer;
    [SerializeField] private HudController _hud;
    [SerializeField] private EndGameController _endGame;
    [SerializeField] private float _idleHintDelay = 5f;
    [SerializeField] private SpriteRenderer _background;
    [SerializeField] private float _cameraMargin = 1f;
    [SerializeField] private SpriteRenderer _cellTilePrefab;
    
    private LevelDefinition _level;
    
    private bool _hasStart;
    private int _startX, _startY;
    private Vector3 _startWorldPos;
    private bool _swipeConsumed;
    private Board _board;
    private GoalTracker _goalTracker;
    private int _movesRemaining;
    private bool _levelEnded;
    
    private float _idleTimer;
    private PieceView _hintView1;
    private PieceView _hintView2;
    
    
    void Start()
    {
        int index = Mathf.Clamp(LevelProgress.CurrentIndex, 0, _levels.Length - 1);
        _level = _levels[index];
        _board = new Board(_level.Width, _level.Height);
        _turnPlayer.Init(_board);
        FitCameraToBoard();
            
        OrganizationLogic.GetNewPieceToEmpty(_board, new SystemRandom());

        foreach (var obstacle in _level.Obstacles)
        {
            IObstacleSpawner spawner = ObstacleSpawnerRegistry.Get(obstacle.Type);
            if (spawner != null && spawner.Phase == ObstacleSpawnPhase.BeforeFill)
            {
                spawner.Spawn(_board, _turnPlayer, obstacle);
            }
        }

        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                Piece piece = _board.GetCell(x, y);
                if (BoardLogic.IsVoid(piece))
                {
                    continue;
                }
                
                if (_cellTilePrefab != null)
                {
                    SpriteRenderer tile = Instantiate(_cellTilePrefab, _turnPlayer.CellToWorld(x, y), Quaternion.identity, transform);
                    tile.sortingOrder = SortingOrders.CellTile;
                }
                
                _turnPlayer.SpawnPieceView(piece, _turnPlayer.CellToWorld(x, y));
            }
        }

        foreach (var obstacle in _level.Obstacles)
        {
            IObstacleSpawner spawner = ObstacleSpawnerRegistry.Get(obstacle.Type);
            if (spawner != null && spawner.Phase == ObstacleSpawnPhase.AfterFill)
            {
                spawner.Spawn(_board, _turnPlayer, obstacle);
            }
        }
        
        _goalTracker = new GoalTracker(_board, _level.Goals);
        _movesRemaining = _level.MoveLimit;
        _hud.SetMoves(_movesRemaining);
        _hud.SetGoals(_goalTracker);
        
        var initialResult = TurnResolver.Resolve(_board, null);
        ApplyTurnResult(initialResult, false);
        StartCoroutine(PlayInitialTurn(initialResult));
    }

    void Update()
    {
        if (Pointer.current == null)
        {
            return;
        }

        if (!_turnPlayer.IsBusy && !_levelEnded && _hintView1 == null)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleHintDelay)
            {
                ShowHint();
            }
        }
        
        if (Pointer.current.press.wasPressedThisFrame)
        {
            _idleTimer = 0f;
            ClearHint();

            _startWorldPos = PointerWorldPosition();
            (_startX, _startY) = _turnPlayer.WorldToCell(_startWorldPos);
            _hasStart = _startX >= 0 && _startX < _board.Width && _startY >= 0 && _startY < _board.Height;
            _swipeConsumed = false;
        }

        if (_hasStart && !_swipeConsumed && Pointer.current.press.isPressed)
        {
            Vector3 delta = PointerWorldPosition() - _startWorldPos;
            float threshold = _turnPlayer.CellSize * _swipeThresholdCells;

            if (delta.magnitude >= threshold)
            {
                int targetX = _startX;
                int targetY = _startY;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    targetX += delta.x > 0 ? 1 : -1;
                else
                    targetY += delta.y > 0 ? -1 : 1;

                _swipeConsumed = true;
                TrySwap(_startX, _startY, targetX, targetY);
            }
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            if (_hasStart && !_swipeConsumed)
            {
                TryActivate(_startX, _startY);
            }
            _hasStart = false;
            _swipeConsumed = false;
        }
    }
    
    private IEnumerator PlayInitialTurn(List<TurnStepData> result)
    {
        yield return StartCoroutine(_turnPlayer.PlayTapTurn(result));
        ShowEndGameIfNeeded();
    }
    
    private void ApplyTurnResult(List<TurnStepData> result, bool consumesMove)
    {
        if (result.Count == 0)
        {
            return;
        }

        if (consumesMove)
        {
            _movesRemaining--;
        }

        foreach (var step in result)
        {
            if (step is GoalProgressData goalProgress)
            {
                _goalTracker.ApplyProgress(goalProgress);
                break;
            }
        }

        _hud.SetMoves(_movesRemaining);
        _hud.SetGoals(_goalTracker);
    }
    
    private void ShowEndGameIfNeeded()
    {
        if (_goalTracker.IsComplete())
        {
            _levelEnded = true;
            _endGame.ShowWin();
        }
        else if (_movesRemaining <= 0)
        {
            _levelEnded = true;
            _endGame.ShowLose();
        }
    }
    
    public void TrySwap(int x1, int y1, int x2, int y2)
    {
        if (_levelEnded)
        {
            return;
        }
        
        if (_turnPlayer.IsBusy)
        {
            Debug.Log("Still resolving previous turn, ignoring this swap");
            return;
        }

        if (!BoardLogic.IsValidIndex(x1, y1, _board)) return;
        if (!BoardLogic.IsValidIndex(x2, y2, _board)) return;

        Piece piece1 = _board.GetCell(x1, y1);
        Piece piece2 = _board.GetCell(x2, y2);
        if (piece1 == null || piece2 == null) return;

        if (BoardLogic.IsCellSwapBlocked(_board, x1, y1) || BoardLogic.IsCellSwapBlocked(_board, x2, y2)) return;
        if (!BoardLogic.IsPieceMoveable(piece1) || !BoardLogic.IsPieceMoveable(piece2)) return;

        var command = new SwapCommand(_board, x1, y1, x2, y2);
        var result = TurnResolver.Resolve(_board, command, targetPriority: _goalTracker);
        bool isCombo = false;
        foreach (var step in result)
        {
            if (step is SpecialActivationData sad && sad.IsCombo)
            {
                isCombo = true;
                break;
            }
        }
        
        ApplyTurnResult(result, true);
        StartCoroutine(PlayTurnAndCheckShuffle(_turnPlayer.PlaySwapTurn(piece1.Id, piece2.Id, x1, y1, x2, y2, result, isCombo)));
    }
    
    public void TryActivate(int x, int y)
    {
        if (_levelEnded)
        {
            return;
        }
        
        if (_turnPlayer.IsBusy)
        {
            Debug.Log("Still resolving previous turn, ignoring this tap");
            return;
        }

        if (!BoardLogic.IsValidIndex(x, y, _board)) return;

        Piece piece = _board.GetCell(x, y);
        if (piece == null || piece.VisualType != PieceVisualType.Special) return;
        if (BoardLogic.IsPieceLocked(piece)) return;

        var command = new TapCommand(x, y);
        var result = TurnResolver.Resolve(_board, command, targetPriority: _goalTracker);
        ApplyTurnResult(result, true);
        
        StartCoroutine(PlayTurnAndCheckShuffle(_turnPlayer.PlayTapTurn(result)));
    }
    
    private IEnumerator PlayTurnAndCheckShuffle(IEnumerator turnRoutine)
    {
        yield return StartCoroutine(turnRoutine);

        ShowEndGameIfNeeded();
        if (_levelEnded)
        {
            yield break;
        }
        
        if (!MoveLogic.HasAvailableMove(_board))
        {
            var shuffleSteps = TurnResolver.ResolveShuffle(_board, new SystemRandom());
            yield return StartCoroutine(_turnPlayer.PlayShuffleTurn(shuffleSteps));
        }
    }
    
    private void ShowHint()
    {
        var move = MoveLogic.FindAvailableMove(_board);
        if (move == null)
        {
            return;
        }

        var (x1, y1, x2, y2) = move.Value;
        _hintView1 = _turnPlayer.GetView(_board.GetCell(x1, y1).Id);
        _hintView2 = _turnPlayer.GetView(_board.GetCell(x2, y2).Id);
        _hintView1?.PlayHintPulse();
        _hintView2?.PlayHintPulse();
    }

    private void ClearHint()
    {
        _hintView1?.StopHintPulse();
        _hintView2?.StopHintPulse();
        _hintView1 = null;
        _hintView2 = null;
    }
    
    private void FitCameraToBoard()
    {
        Camera cam = Camera.main;
        float aspect = cam.aspect;

        float heightNeeded = (_level.Height + _cameraMargin) / 2f;
        float widthNeeded = (_level.Width + _cameraMargin) / (2f * aspect);
        cam.orthographicSize = Mathf.Max(heightNeeded, widthNeeded);

        float visibleHeight = cam.orthographicSize * 2f;
        float visibleWidth = visibleHeight * aspect;

        Vector2 nativeSize = _background.sprite.bounds.size;
        _background.transform.localScale = new Vector3(visibleWidth / nativeSize.x, visibleHeight / nativeSize.y, 1f);
        _background.sortingOrder = SortingOrders.Background;
    }
    
    private Vector3 PointerWorldPosition()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        world.z = 0f;
        return world;
    }
    
    
    
    
    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public int LevelCount => _levels.Length;

    public void DebugAddMoves(int amount)
    {
        if (_levelEnded) return;
        _movesRemaining += amount;
        _hud.SetMoves(_movesRemaining);
    }
#endif
}
