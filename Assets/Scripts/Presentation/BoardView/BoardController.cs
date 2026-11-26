using System;
using System.Collections;
using System.Collections.Generic;
using LogicLibrary;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardController : MonoBehaviour
{
    public event Action<TurnSummary> TurnStarted;
    public event Action<int> MovesChanged;
    public event Action HintShown;
    public event Action<bool> LevelEnded; 
    
    [SerializeField, Range(0.1f, 1f)] private float _swipeThresholdCells = 0.35f;

    [SerializeField] private TurnPlayer _turnPlayer;
    [SerializeField] private HudController _hud;
    [SerializeField] private PopupManager _popups;
    [SerializeField] private int _extraMovesAmount = 5;
    [SerializeField] private float _idleHintDelay = 5f;
    [SerializeField] private SpriteRenderer _background;
    [SerializeField] private RectTransform _topPanel;
    [SerializeField] private RectTransform _bottomPanel;
    [SerializeField] private float _boardPadding = 0.25f;
    [SerializeField] private BoosterBarController _boosterBar;
    
    [SerializeField] private GameAssets _assets;
    [SerializeField] private AnimationSettings _anim;
    [SerializeField] private SettingsDropdownView _settings;
    
    private LevelData _level; 
    
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
    
    private int _levelIndex;
    private bool _extraMovesUsed;
    
    public event Action LevelResumed;
    
    private void OnValidate()
    {
        if (_turnPlayer == null) _turnPlayer = GetComponent<TurnPlayer>();
        if (_hud == null) _hud = AutoRef.InScene<HudController>();
        if (_popups == null) _popups = AutoRef.InScene<PopupManager>();
        if (_boosterBar == null) _boosterBar = AutoRef.InScene<BoosterBarController>();
        if (_background == null) _background = GameObject.Find("env_background")?.GetComponent<SpriteRenderer>();
        if (_topPanel == null) _topPanel = GameObject.Find("ui_panel_top")?.GetComponent<RectTransform>();
        if (_bottomPanel == null) _bottomPanel = GameObject.Find("ui_panel_bottom")?.GetComponent<RectTransform>();
        if (_settings == null) _settings = AutoRef.InScene<SettingsDropdownView>();
        
#if UNITY_EDITOR
        if (_assets == null) _assets = AutoRef.Asset<GameAssets>();
        if (_anim == null) _anim = AutoRef.Asset<AnimationSettings>();
#endif
    }
    
    void Start()
    {
        AudioManager.EnsureCreated();

        int index = Mathf.Clamp(LevelProgress.CurrentIndex, 0, _assets.Levels.Count - 1);
        _levelIndex = index;
        _level = _assets.Levels.Get(_levelIndex);
        _board = LevelBoardBuilder.Build(_level, new SystemRandom());
        _turnPlayer.Init(_board);
        FitCameraToBoard();
        SpawnBoardViews();

        _boosterBar.InstantBoosterRequested += OnInstantBooster;

        _goalTracker = new GoalTracker(_board, LevelBoardBuilder.BuildGoals(_level));
        _movesRemaining = _level.moves;
        _hud.SetMoves(_movesRemaining);
        _hud.BuildGoals(_goalTracker);
        _hud.SetLevel(index + 1);
        _turnPlayer.GoalFlyer.Bind(_hud);

        var initialResult = TurnResolver.Resolve(_board, null);
        ApplyTurnResult(initialResult, false);
        StartCoroutine(PlayInitialTurn(initialResult));
        if (_hasStart && _boosterBar.Selected.HasValue)
        {
            _turnPlayer.ShowTargetReticle(_startX, _startY);
        }
        
        //Debug.Log($"[LevelDebug] CurrentIndex={LevelProgress.CurrentIndex} Count={_assets.Levels.Count} Type={_assets.Levels.GetType().Name} Dir={Application.persistentDataPath}/levels");
    }

    void Update()
    {
        if (_popups.IsAnyOpen)
        {
            _hasStart = false;
            _turnPlayer.HideTargetReticle();
            return;
        }
        
        if (Pointer.current == null)
        {
            return;
        }

        if (_settings != null && _settings.IsOpen)
        {
            _hasStart = false;
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

        if (_hasStart && !_swipeConsumed && !_boosterBar.Selected.HasValue && Pointer.current.press.isPressed)
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
            _turnPlayer.HideTargetReticle();
            if (_hasStart && !_swipeConsumed)
            {
                if (_boosterBar.Selected.HasValue)
                {
                    TryUseBooster(_boosterBar.Selected.Value, _startX, _startY);
                }
                else
                {
                    TryActivate(_startX, _startY);
                }
            }
            _hasStart = false;
            _swipeConsumed = false;
        }
        
        if (_hasStart && _boosterBar.Selected.HasValue)
        {
            _turnPlayer.ShowTargetReticle(_startX, _startY);
        }
    }
    
    private void SpawnBoardViews()
    {
        bool flipFlop = false;
        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                Piece piece = _board.GetCell(x, y);

                if (piece == null)
                {
                    continue;
                }
                
                if (BoardLogic.IsVoid(piece))
                {
                    continue;
                }

                Vector3 position = _turnPlayer.CellToWorld(x, y);
                if (_assets.CellTilePrefab != null)
                {
                    CellTile tile = Instantiate(_assets.CellTilePrefab, position, Quaternion.identity, transform);
                    if (flipFlop)
                    {
                        tile.SetCellDarkConfig();
                    }
                    else
                    {
                        tile.SetCellLightConfig();
                    }
                    flipFlop = !flipFlop;
                }

                _turnPlayer.SpawnPieceView(piece, position);

                FloorPiece floor = _board.GetFloor(x, y);
                if (floor != null)
                {
                    _turnPlayer.SpawnPieceView(floor, position).SetSortingOrder(SortingOrders.Floor);
                }
            }
        }

        foreach (OverlayPiece overlay in _board.OverlayPieces)
        {
            SpawnOverlayView(overlay);
        }
    }
    
    private void SpawnOverlayView(OverlayPiece overlay)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;
        
        foreach (var (x, y) in overlay.CoveredCells)
        {
            minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
            minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
        }

        Vector3 center = (_turnPlayer.CellToWorld(minX, minY) + _turnPlayer.CellToWorld(maxX, maxY)) / 2f;
        PieceView view = _turnPlayer.SpawnPieceView(overlay, center);

        int width = maxX - minX + 1, height = maxY - minY + 1;
        if (width > 1 || height > 1)
        {
            view.SetSizeInCells(width, height);
        }
        view.SetSortingOrder(SortingOrders.Overlay);
    }
    
    private void OnDestroy()
    {
        if (_boosterBar != null)
        {
            _boosterBar.InstantBoosterRequested -= OnInstantBooster;
        }
    }

    private IEnumerator PlayInitialTurn(List<TurnStepData> result)
    {
        var allCells = AllCells();
        _turnPlayer.LockCells(allCells);
        yield return StartCoroutine(_turnPlayer.PlayTapTurn(result));
        
        _turnPlayer.UnlockCells(allCells);
        yield return StartCoroutine(FinishTurn());
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
        MovesChanged?.Invoke(_movesRemaining);
    }
    
    private IEnumerator FinishTurn()
    {
        bool ending = _goalTracker.IsComplete() || _movesRemaining <= 0;
        if (ending)
        {
            _levelEnded = true;
            yield return StartCoroutine(_turnPlayer.GoalFlyer.WaitForFlights());
            _hud.SyncGoals(_goalTracker);
        }
        ShowEndGameIfNeeded();
    }
    
    private void ShowEndGameIfNeeded()
    {
        if (_goalTracker.IsComplete())
        {
            _levelEnded = true;
            _boosterBar.SetInteractable(false);
            LevelEnded?.Invoke(true);

            int stars = CalculateStars();
            int gained = LevelProgress.SaveStars(_levelIndex, stars);
            WheelProgress.AddStars(gained, _assets.Wheel.StarsPerSpin);
            _popups.Show<WinPopup>().Setup(_levelIndex + 1, stars);
        }
        else if (_movesRemaining <= 0)
        {
            _levelEnded = true;
            _boosterBar.SetInteractable(false);
            LevelEnded?.Invoke(false);

            _popups.Show<LosePopup>().Setup(_levelIndex + 1, _extraMovesAmount, !_extraMovesUsed, ContinueWithExtraMoves, _goalTracker);
        }
    }
    
    private int CalculateStars()
    {
        if (_extraMovesUsed)
        {
            return 1;
        }

        LevelStars stars = _level.stars;
        if (stars != null && stars.three > 0)
        {
            if (_movesRemaining >= stars.three)
            {
                return 3;
            }
            if (_movesRemaining >= stars.two)
            {
                return 2;
            }
            return 1;
        }

        float ratio = (float)_movesRemaining / _level.moves;
        if (ratio >= 0.15f)
        {
            return 3;
        }
        if (ratio >= 0.05f)
        {
            return 2;
        }
        return 1;
    }

    private void ContinueWithExtraMoves()
    {
        _extraMovesUsed = true;
        _movesRemaining += _extraMovesAmount;
        _levelEnded = false;
        _boosterBar.SetInteractable(true);
        _hud.SetMoves(_movesRemaining);
        MovesChanged?.Invoke(_movesRemaining);
        LevelResumed?.Invoke();
    }
    
    public void TrySwap(int x1, int y1, int x2, int y2)
    {
        if (_levelEnded)
        {
            return;
        }
        
        if (!BoardLogic.IsValidIndex(x1, y1, _board))
        {
            return;
        }
        
        if (!BoardLogic.IsValidIndex(x2, y2, _board))
        {
            return;
        }
        
        if (!_turnPlayer.AreCellsFree((x1, y1), (x2, y2)))
        {
            Debug.Log("Those cells are still busy, ignoring this swap");
            return;
        }
       
        var command = new SwapCommand(_board, x1, y1, x2, y2);
        if (!command.Validate(_board))
        {
            return;
        }

        Piece piece1 = _board.GetCell(x1, y1);
        Piece piece2 = _board.GetCell(x2, y2);

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
        
        var lockedCells = CollectAffectedCells(result);
        lockedCells.Add((x1, y1));
        lockedCells.Add((x2, y2));
        _turnPlayer.LockCells(lockedCells);
        
        ApplyTurnResult(result, true);
        TurnStarted?.Invoke(TurnSummary.From(result));
        StartCoroutine(PlayTurnAndCheckShuffle(lockedCells, _turnPlayer.PlaySwapTurn(piece1?.Id, piece2?.Id, x1, y1, x2, y2, result, isCombo)));
    }
    
    public void TryActivate(int x, int y)
    {
        if (_levelEnded)
        {
            return;
        }
        
        if (!_turnPlayer.AreCellsFree((x, y)))
        {
            Debug.Log("That cell is still busy, ignoring this tap");
            return;
        }

        Piece piece = _board.GetCell(x, y);
        if (piece == null || piece.VisualType != PieceVisualType.Special) return;
        if (BoardLogic.IsPieceLocked(piece)) return;

        var command = new TapCommand(x, y);
        var result = TurnResolver.Resolve(_board, command, targetPriority: _goalTracker);

        var lockedCells = CollectAffectedCells(result);
        lockedCells.Add((x, y));
        _turnPlayer.LockCells(lockedCells);

        ApplyTurnResult(result, true);
        TurnStarted?.Invoke(TurnSummary.From(result));
        StartCoroutine(PlayTurnAndCheckShuffle(lockedCells, _turnPlayer.PlayTapTurn(result)));
    }
    
    private void TryUseBooster(BoosterType type, int x, int y)
    {
        if (_levelEnded || !_turnPlayer.AreCellsFree((x, y)))
        {
            return;
        }

        var command = new BoosterCommand(type, x, y);
        var result = TurnResolver.Resolve(_board, command, targetPriority: _goalTracker);
        if (result.Count == 0)
        {
            return;
        }

        _boosterBar.TryConsume(type);
        ClearHint();
        _idleTimer = 0f;
        
        var lockedCells = CollectAffectedCells(result);
        lockedCells.Add((x, y));
        _turnPlayer.LockCells(lockedCells);
        
        ApplyTurnResult(result, false);
        TurnStarted?.Invoke(TurnSummary.From(result));
        StartCoroutine(PlayTurnAndCheckShuffle(lockedCells, _turnPlayer.PlayTapTurn(result)));
    }
    
    private void OnInstantBooster(BoosterType type)
    {
        if (type != BoosterType.Shuffle || _levelEnded || _turnPlayer.IsBusy)
        {
            return;
        }
        if (!_boosterBar.TryConsume(type))
        {
            return;
        }

        ClearHint();
        _idleTimer = 0f;
        var steps = TurnResolver.ResolveShuffle(_board, new SystemRandom());
        
        var allCells = AllCells();
        _turnPlayer.LockCells(allCells);
        StartCoroutine(PlayTurnAndCheckShuffle(allCells, _turnPlayer.PlayShuffleTurn(steps)));
    }
    
    private IEnumerator PlayTurnAndCheckShuffle(HashSet<(int x, int y)> lockedCells, IEnumerator turnRoutine)
    {
        yield return StartCoroutine(turnRoutine);
        _turnPlayer.UnlockCells(lockedCells);
        
        yield return StartCoroutine(FinishTurn());
        if (_levelEnded)
        {
            yield break;
        }
        
        if (!_turnPlayer.IsBusy && !MoveLogic.HasAvailableMove(_board))
        {
            var shuffleSteps = TurnResolver.ResolveShuffle(_board, new SystemRandom());
            var allCells = AllCells();
            _turnPlayer.LockCells(allCells);
            yield return StartCoroutine(_turnPlayer.PlayShuffleTurn(shuffleSteps));
            _turnPlayer.UnlockCells(allCells);
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
        Piece first = _board.GetCell(x1, y1);
        Piece second = _board.GetCell(x2, y2);
        _hintView1 = first == null ? null : _turnPlayer.GetView(first.Id);
        _hintView2 = second == null ? null : _turnPlayer.GetView(second.Id);
        _hintView1?.PlayHintPulse();
        _hintView2?.PlayHintPulse();
        HintShown?.Invoke();
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
        Canvas.ForceUpdateCanvases();

        float areaTop = _topPanel != null ? ViewportEdge(_topPanel, top: false) : 0.78f;
        float areaBottom = _bottomPanel != null ? ViewportEdge(_bottomPanel, top: true) : 0.16f;
        float areaHeight = Mathf.Max(0.1f, areaTop - areaBottom);

        float boardWidth = _level.width * _turnPlayer.CellSize + _boardPadding * 2f;
        float boardHeight = _level.height * _turnPlayer.CellSize + _boardPadding * 2f;

        float sizeForHeight = boardHeight / (2f * areaHeight);
        float sizeForWidth = boardWidth / (2f * cam.aspect);
        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);

        float visibleHeight = cam.orthographicSize * 2f;
        float visibleWidth = visibleHeight * cam.aspect;
        float areaCenter = (areaTop + areaBottom) * 0.5f;
        Vector3 camPos = cam.transform.position;
        _turnPlayer.SetBoardOffset(new Vector3(camPos.x, camPos.y + (areaCenter - 0.5f) * visibleHeight, 0f));

        Vector2 nativeSize = _background.sprite.bounds.size;
        _background.transform.localScale = new Vector3(visibleWidth / nativeSize.x, visibleHeight / nativeSize.y, 1f);
        _background.sortingOrder = SortingOrders.Background;
    }
    
    private static float ViewportEdge(RectTransform panel, bool top)
    {
        var corners = new Vector3[4];
        panel.GetWorldCorners(corners);

        Canvas canvas = panel.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector3 corner = top ? corners[1] : corners[0];
        return RectTransformUtility.WorldToScreenPoint(uiCamera, corner).y / Screen.height;
    }
    
    private Vector3 PointerWorldPosition()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        world.z = 0f;
        return world;
    }
    
    private HashSet<(int x, int y)> CollectAffectedCells(List<TurnStepData> steps)
    {
        var columns = new HashSet<int>();

        void AddColumn(int x)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int cx = x + dx;
                if (cx >= 0 && cx < _board.Width)
                {
                    columns.Add(cx);
                }
            }
        }

        foreach (var step in steps)
        {
            switch (step)
            {
                case ClearPieceData clear:
                {
                    foreach (var hit in clear.Hits)
                    {
                        AddColumn(hit.X);
                    }
                    break;
                }
                case ClearOverlayData overlay:
                {
                    foreach (var hit in overlay.Hits)
                    {
                        AddColumn(hit.X);
                    }
                    break;
                }
                case CollectStepData collect:
                {
                    foreach (var hit in collect.Hits)
                    {
                        AddColumn(hit.X);
                    }
                    break;
                }
                case SpecialActivationData activated:
                {
                    AddColumn(activated.X);
                    if (activated.TargetX.HasValue)
                    {
                        AddColumn(activated.TargetX.Value);
                    }
                    if (activated.NeighborCells != null)
                    {
                        foreach (var (nx, _) in activated.NeighborCells)
                        {
                            AddColumn(nx);
                        }
                    }
                    if (activated.Targets != null)
                    {
                        foreach (var (tx, _, _) in activated.Targets)
                        {
                            AddColumn(tx);
                        }
                    }
                    break;
                }
                case SpecialCreatedData created:
                {
                    AddColumn(created.X);
                    break;
                }
                case FallStepData fall:
                {
                    foreach (var move in fall.Moves)
                    {
                        AddColumn(move.toX);
                    }
                    break;
                }
                case RefillStepData refill:
                {
                    foreach (var move in refill.Moves)
                    {
                        AddColumn(move.toX);
                    }
                    break;
                }
                case ComboMergeData merge:
                {
                    AddColumn(merge.ToX);
                    break;
                }
            }
        }

        var cells = new HashSet<(int x, int y)>();
        foreach (int x in columns)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                cells.Add((x, y));
            }
        }
        return cells;
    }

    private HashSet<(int x, int y)> AllCells()
    {
        var cells = new HashSet<(int x, int y)>();
        for (int x = 0; x < _board.Width; x++)
        {
            for (int y = 0; y < _board.Height; y++)
            {
                cells.Add((x, y));
            }
        }
        return cells;
    }
    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public int LevelCount => _assets.Levels.Count;

    public void DebugAddMoves(int amount)
    {
        if (_levelEnded) return;
        _movesRemaining += amount;
        _hud.SetMoves(_movesRemaining);
    }
#endif
}
