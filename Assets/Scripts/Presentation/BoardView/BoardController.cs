using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardController : MonoBehaviour
{
    [SerializeField] private PieceView _piecePrefab;
    [SerializeField] private float _cellSize = 1f;
    
    private Dictionary<int, PieceView> _pieceById = new Dictionary<int, PieceView>();

    private bool _hasStart;
    private int _startX, _startY;
    private Vector3 _startWorldPos;
    
    private Board _board;
    void Start()
    {
        _board = new Board(5, 5);
        LogicLibrary.OrganizationLogic.GetNewPieceToEmpty(_board);
        
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 2f;
        
        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                Vector3 worldPos = new Vector3((x - offsetX) * _cellSize, (y - offsetY) * _cellSize, 0);
                PieceView instance = Instantiate(_piecePrefab, worldPos, Quaternion.identity, transform);
                Piece piece = _board.GetCell(x, y);
                instance.Setup(piece);
                _pieceById[piece.Id] = instance;
            }
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

            int targetX = _startX;
            int targetY = _startY;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                targetX += delta.x > 0 ? 1 : -1;
            else
                targetY += delta.y > 0 ? 1 : -1;

            if (!LogicLibrary.BoardLogic.IsValidIndex(_startX, _startY, _board))
            {
                Debug.Log("Invalid index 1");
                return;
            }
            
            if (!LogicLibrary.BoardLogic.IsValidIndex(targetX, targetY, _board))
            {
                Debug.Log("Invalid index 2");
                return;
            }
            
            var command = new SwapCommand(_board, _startX, _startY, targetX, targetY);
            var result = TurnResolver.Resolve(_board, command);

            foreach (var pieceView in _pieceById)
            {
                Destroy(pieceView.Value);
            }
            _pieceById.Clear();
            
            float offsetX = (_board.Width - 1) / 2f;
            float offsetY = (_board.Height - 1) / 2f;
        
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    Vector3 worldPos = new Vector3((x - offsetX) * _cellSize, (y - offsetY) * _cellSize, 0);
                    PieceView instance = Instantiate(_piecePrefab, worldPos, Quaternion.identity, transform);
                    Piece piece = _board.GetCell(x, y);
                    instance.Setup(piece);
                    _pieceById[piece.Id] = instance;
                }
            }
            
            _hasStart = false;
        }
    }
    
    private (int, int) WorldToCell(Vector3 worldPos)
    {
        float offsetX = (_board.Width - 1) / 2f;
        float offsetY = (_board.Height - 1) / 2f;

        int x = Mathf.RoundToInt(worldPos.x / _cellSize + offsetX);
        int y = Mathf.RoundToInt(worldPos.y / _cellSize + offsetY);
        return (x, y);
    }
}
