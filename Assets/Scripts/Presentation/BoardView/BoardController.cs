using System.Collections;
using System.Collections.Generic;
using LogicLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Vector2Int _boardSize;
    [SerializeField] private TurnPlayer _turnPlayer;
    
    private bool _hasStart;
    private int _startX, _startY;
    private Vector3 _startWorldPos;
    private Board _board;
    
    void Start()
    {
        _board = new Board(_boardSize.x, _boardSize.y);
        _turnPlayer.Init(_board);

        OrganizationLogic.GetNewPieceToEmpty(_board, new SystemRandom());
        _board.SetCell(2, 2, new BoxPiece(_board.NextId(), PieceColor.Empty));

        for (int x = 0; x < _board.Width; x++)
        for (int y = 0; y < _board.Height; y++)
        {
            Piece piece = _board.GetCell(x, y);
            _turnPlayer.SpawnPieceView(piece, _turnPlayer.CellToWorld(x, y));
        }

        HashSet<(int x, int y)> overlayCells = new HashSet<(int x, int y)> { (1, 1), (1, 0), (2, 1), (2, 0) };
        LockPiece lockPiece = new LockPiece(_board.NextId(), overlayCells);
        _board.AddOverlayPiece(lockPiece);

        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        Vector3 overlayCenter = Vector3.zero;
        foreach (var (x, y) in overlayCells)
        {
            overlayCenter += _turnPlayer.CellToWorld(x, y);
            minX = Mathf.Min(minX, x);
            minY = Mathf.Min(minY, y);
            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);

            Piece covered = _board.GetCell(x, y);
            if (covered != null)
                BoardLogic.AddFlag(covered, PieceFlags.Locked);
        }
        overlayCenter /= overlayCells.Count;

        PieceView lockView = _turnPlayer.SpawnPieceView(lockPiece, overlayCenter);
        lockView.SetSizeInCells(maxX - minX + 1, maxY - minY + 1);
        lockView.SetSortingOrder(1);

        // TODO(level-editor): temponary, after the level editor, will be this line remove.
        TrySwap(0, 0, 0, 1);
    }

    void Update()
    {
        // TODO(android-input): Mouse only.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            _startWorldPos = worldPos;
            (_startX, _startY) = _turnPlayer.WorldToCell(worldPos);
            _hasStart = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && _hasStart)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
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
    
    public void TrySwap(int x1, int y1, int x2, int y2)
    {
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

        if (BoardLogic.IsPieceLocked(piece1) || BoardLogic.IsPieceLocked(piece2)) return;
        if (!BoardLogic.IsPieceMoveable(piece1) || !BoardLogic.IsPieceMoveable(piece2)) return;

        var command = new SwapCommand(_board, x1, y1, x2, y2);
        var result = TurnResolver.Resolve(_board, command);

        StartCoroutine(PlayTurnAndCheckShuffle(_turnPlayer.PlaySwapTurn(piece1.Id, piece2.Id, x1, y1, x2, y2, result)));
    }
    
    public void TryActivate(int x, int y)
    {
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
        var result = TurnResolver.Resolve(_board, command);

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
}
