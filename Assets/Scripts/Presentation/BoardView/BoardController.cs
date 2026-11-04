using System.Collections;
using System.Collections.Generic;
using LogicLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardController : MonoBehaviour
{
    [SerializeField] private LevelDefinition _level;
    [SerializeField] private TurnPlayer _turnPlayer;
    [SerializeField] private HudController _hud;
    [SerializeField] private EndGameController _endGame;
    [SerializeField] private float _idleHintDelay = 6f;
    
    private bool _hasStart;
    private int _startX, _startY;
    private Vector3 _startWorldPos;
    private Board _board;
    private GoalTracker _goalTracker;
    private int _movesRemaining;
    private bool _levelEnded;
    
    private float _idleTimer;
    private PieceView _hintView1;
    private PieceView _hintView2;
    
    void Start()
    {
        _board = new Board(_level.Width, _level.Height);
        _turnPlayer.Init(_board);

        OrganizationLogic.GetNewPieceToEmpty(_board, new SystemRandom());

        foreach (var obstacle in _level.Obstacles)
        {
            if (obstacle.Type != ObstacleType.Box) continue;
            Vector2Int cell = obstacle.Cells[0];
            _board.SetCell(cell.x, cell.y, new BoxPiece(_board.NextId(), PieceColor.Empty));
        }

        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                Piece piece = _board.GetCell(x, y);
                _turnPlayer.SpawnPieceView(piece, _turnPlayer.CellToWorld(x, y));
            }
        }
        
        foreach (var obstacle in _level.Obstacles)
        {
            if (obstacle.Type != ObstacleType.Lock) continue;
            SpawnLockOverlay(obstacle.Cells[0]);
        }
        
        foreach (var obstacle in _level.Obstacles)
        {
            if (obstacle.Type != ObstacleType.Grass) continue;
            SpawnGrassOverlay(obstacle.Cells[0]);
        }
        
        foreach (var obstacle in _level.Obstacles)
        {
            if (obstacle.Type != ObstacleType.Cage) continue;
            SpawnCageOverlay(obstacle.Cells[0]);
        }
        
        _goalTracker = new GoalTracker(_level.Goals);
        _movesRemaining = _level.MoveLimit;
        _hud.SetMoves(_movesRemaining);
        _hud.SetGoals(_goalTracker);
        
        var initialResult = TurnResolver.Resolve(_board, null);
        ApplyTurnResult(initialResult, false);
        StartCoroutine(_turnPlayer.PlayTapTurn(initialResult));
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
            
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            _startWorldPos = worldPos;
            (_startX, _startY) = _turnPlayer.WorldToCell(worldPos);
            _hasStart = true;
        }
        
        if (Pointer.current.press.wasReleasedThisFrame && _hasStart)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector3 endWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            Vector3 delta = endWorldPos - _startWorldPos;

            if (delta.magnitude > .5f)
            {
                int targetX = _startX;
                int targetY = _startY;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    targetX += delta.x > 0 ? 1 : -1;
                else
                    targetY += delta.y > 0 ? -1 : 1;

                TrySwap(_startX, _startY, targetX, targetY);
            }
            else
            {
                TryActivate(_startX, _startY);
            }
            _hasStart = false;
        }
    }
    
    private void SpawnLockOverlay(Vector2Int cell)
    {
        LockPiece lockPiece = new LockPiece(_board.NextId(), cell.x, cell.y);
        _board.AddOverlayPiece(lockPiece);

        Piece covered = _board.GetCell(cell.x, cell.y);
        if (covered != null)
        {
            BoardLogic.AddFlag(covered, PieceFlags.Locked);
        }

        PieceView lockView = _turnPlayer.SpawnPieceView(lockPiece, _turnPlayer.CellToWorld(cell.x, cell.y));
        lockView.SetSortingOrder(1);
    }
    
    private void SpawnGrassOverlay(Vector2Int cell)
    {
        GrassPiece grassPiece = new GrassPiece(_board.NextId(), cell.x, cell.y);
        _board.AddOverlayPiece(grassPiece);

        PieceView grassView = _turnPlayer.SpawnPieceView(grassPiece, _turnPlayer.CellToWorld(cell.x, cell.y));
        grassView.SetSortingOrder(1);
    }
    
    private void SpawnCageOverlay(Vector2Int anchorCell)
    {
        CagePiece cagePiece = new CagePiece(_board.NextId(), anchorCell.x, anchorCell.y);
        _board.AddOverlayPiece(cagePiece);

        foreach (var (x, y) in cagePiece.CoveredCells)
        {
            Piece covered = _board.GetCell(x, y);
            if (covered != null)
            {
                BoardLogic.AddFlag(covered, PieceFlags.Locked);
            }
        }

        Vector3 topLeft = _turnPlayer.CellToWorld(anchorCell.x, anchorCell.y);
        Vector3 bottomRight = _turnPlayer.CellToWorld(anchorCell.x + 1, anchorCell.y + 1);
        Vector3 center = (topLeft + bottomRight) / 2f;

        PieceView cageView = _turnPlayer.SpawnPieceView(cagePiece, center);
        cageView.SetSizeInCells(2, 2);
        cageView.SetSortingOrder(1);
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
        ApplyTurnResult(result, true);
        
        StartCoroutine(PlayTurnAndCheckShuffle(_turnPlayer.PlaySwapTurn(piece1.Id, piece2.Id, x1, y1, x2, y2, result)));
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
}
