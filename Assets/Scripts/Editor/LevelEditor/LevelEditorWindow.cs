using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class LevelEditorWindow : EditorWindow
{
    private const float PanelWidth = 230f;
    private const float BoardPadding = 12f;
    private const int MaxErrorLines = 6;

    private static readonly string[] TabNames = { "Normal", "Overlay", "Floor" };
    private static readonly LevelLayer[] TabLayers = { LevelLayer.Cell, LevelLayer.Overlay, LevelLayer.Floor };
    private static readonly PieceColor[] AllColors =
        ((PieceColor[])Enum.GetValues(typeof(PieceColor))).Where(c => c != PieceColor.Empty).ToArray();

    private static readonly Color TileA = new Color(0.24f, 0.26f, 0.31f);
    private static readonly Color TileB = new Color(0.28f, 0.30f, 0.36f);
    private static readonly Color HoleColor = new Color(0.08f, 0.08f, 0.10f);
    private static readonly Color HoverColor = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color UnknownColor = new Color(0.8f, 0.1f, 0.1f, 0.6f);

    [SerializeField] private LevelAsset _asset;
    [SerializeField] private int _tab;
    [SerializeField] private int _botRunCount = 100;
    
    private List<LevelBot.RunResult> _botResults;
    private string _botLevelKey;
    private double _botSeconds;
    private Task<List<LevelBot.RunResult>> _botTask;
    private CancellationTokenSource _botCancel;
    private Stopwatch _botTimer;
    private string _botPendingKey;
    private int _botDone, _botTotal;
    
    private ILevelElement _brush;
    private LevelEditorPreview _preview;
    private List<string> _errors = new List<string>();
    private string _message;
    private Vector2 _panelScroll;
    private (int x, int y) _lastPainted = (-1, -1);
    private int _newWidth, _newHeight;
    private int _newGoalColor;
    private int _newGoalCount = 20;
    private GUIStyle _codeStyle, _brushStyle;

    private LevelDifficulty? _tuneTarget;
    
    private LevelData Level => _asset != null ? _asset.Data : null;
    private LevelLayer Layer => TabLayers[_tab];

    [MenuItem("Match3/Level Editor")]
    private static void Open() => GetWindow<LevelEditorWindow>("Level Editor");
    
    private void OnEnable()
    {
        _preview = new LevelEditorPreview();
        wantsMouseMove = true;
        Undo.undoRedoPerformed += OnUndoRedo;
        SelectFirstBrush();
        LoadAsset(_asset);
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        CancelBot();
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is LevelAsset asset && asset != _asset)
        {
            LoadAsset(asset);
            Repaint();
        }
    }

    private void OnUndoRedo()
    {
        if (Level != null)
        {
            _newWidth = Level.width;
            _newHeight = Level.height;
            Validate();
        }
        Repaint();
    }

    private void LoadAsset(LevelAsset asset)
    {
        _asset = asset;
        _message = null;
        _errors.Clear();
        _botResults = null;
        CancelBot();
        _tuneTarget = null;
        
        if (Level == null)
        {
            return;
        }

        LevelEditing.EnsureRows(Level);
        if (Level.stars == null)
        {
            Level.stars = new LevelStars();
        }
        _newWidth = Level.width;
        _newHeight = Level.height;
        Validate();
    }

    private void OnGUI()
    {
        DrawTopBar();
        if (Level == null)
        {
            EditorGUILayout.HelpBox("Select a LevelAsset in the Project window, drag one into the field above, or press New.", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        Rect area = GUILayoutUtility.GetRect(100f, 100f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawPanel();
        EditorGUILayout.EndHorizontal();
        DrawMessages();
        DrawBot();

        if (Event.current.type != EventType.Layout)
        {
            Rect board = BoardRect(area, out float cell);
            HandleInput(board, cell);
            if (Event.current.type == EventType.Repaint)
            {
                DrawBoard(board, cell);
            }
        }

        if (AssetPreview.IsLoadingAssetPreviews())
        {
            Repaint();
        }
    }

    private void Change(string undoName, Action change)
    {
        Undo.RecordObject(_asset, undoName);
        change();
        EditorUtility.SetDirty(_asset);
        Validate();
    }

    private void Validate()
    {
        _errors = LevelValidator.Validate(Level);
    }
    
    private void DrawTopBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("◀", EditorStyles.toolbarButton, GUILayout.Width(24f)))
        {
            Step(-1);
        }
        EditorGUI.BeginChangeCheck();
        var asset = (LevelAsset)EditorGUILayout.ObjectField(_asset, typeof(LevelAsset), false, GUILayout.Width(170f));
        if (EditorGUI.EndChangeCheck())
        {
            LoadAsset(asset);
        }
        if (GUILayout.Button("▶", EditorStyles.toolbarButton, GUILayout.Width(24f)))
        {
            Step(1);
        }

        GUILayout.Space(10f);
        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(42f)))
        {
            OpenAndExit(LevelEditorAssets.Create(LevelEditorAssets.NewLevelData()));
        }
        using (new EditorGUI.DisabledScope(_asset == null))
        {
            if (GUILayout.Button("Copy", EditorStyles.toolbarButton, GUILayout.Width(42f)))
            {
                OpenAndExit(LevelEditorAssets.Create(LevelEditorAssets.CopyData(Level)));
            }
            if (GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(50f)))
            {
                DeleteCurrent();
            }
            GUILayout.Space(10f);
            if (GUILayout.Button("Generate…", EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                LevelGeneratorWindow.Open();
            }
        }

        GUILayout.FlexibleSpace();
        using (new EditorGUI.DisabledScope(_asset == null || EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(42f)))
            {
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button("▶ Play", EditorStyles.toolbarButton, GUILayout.Width(56f)))
            {
                PlayCurrent();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Step(int direction)
    {
        LevelAsset[] all = LevelEditorAssets.All();
        if (all.Length == 0)
        {
            return;
        }
        int index = Array.IndexOf(all, _asset);
        LoadAsset(all[Mathf.Clamp(index + direction, 0, all.Length - 1)]);
        GUI.FocusControl(null);
    }

    private void DeleteCurrent()
    {
        if (!EditorUtility.DisplayDialog("Delete level", $"Delete {_asset.name}? This can't be undone.", "Delete", "Cancel"))
        {
            return;
        }

        int index = Array.IndexOf(LevelEditorAssets.All(), _asset);
        LevelEditorAssets.Delete(_asset);
        LevelAsset[] all = LevelEditorAssets.All();
        OpenAndExit(all.Length > 0 ? all[Mathf.Clamp(index, 0, all.Length - 1)] : null);
    }

    private void PlayCurrent()
    {
        if (_errors.Count > 0 && !EditorUtility.DisplayDialog("Level has errors", "This level has validation errors. Play anyway?", "Play", "Cancel"))
        {
            return;
        }
        LevelEditorAssets.Play(_asset);
        GUIUtility.ExitGUI();
    }

    private void OpenAndExit(LevelAsset asset)
    {
        LoadAsset(asset);
        Repaint();
        GUIUtility.ExitGUI();
    }
    
    private void DrawPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(PanelWidth));
        _panelScroll = EditorGUILayout.BeginScrollView(_panelScroll);

        DrawBrushes();
        GUILayout.Space(10f);
        DrawSettings();
        GUILayout.Space(10f);
        DrawGoals();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawBrushes()
    {
        int tab = GUILayout.Toolbar(_tab, TabNames);
        if (tab != _tab)
        {
            _tab = tab;
            SelectFirstBrush();
        }

        EditorGUIUtility.SetIconSize(new Vector2(28f, 28f));
        foreach (ILevelElement element in LevelElements.ByLayer(Layer))
        {
            DrawBrushButton(element, new GUIContent(" " + element.DisplayName, _preview.GetTexture(element)));
        }
        GUILayout.Space(4f);
        DrawBrushButton(null, new GUIContent($" Eraser ({TabNames[_tab]})"));
        EditorGUIUtility.SetIconSize(Vector2.zero);
    }

    private void DrawBrushButton(ILevelElement element, GUIContent content)
    {
        _brushStyle ??= new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };

        bool selected = _brush == element;
        if (GUILayout.Toggle(selected, content, _brushStyle, GUILayout.Height(34f)) && !selected)
        {
            _brush = element;
        }
    }

    private void SelectFirstBrush()
    {
        _brush = LevelElements.ByLayer(Layer).FirstOrDefault();
    }

    private void DrawSettings()
    {
        GUILayout.Label("Level", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Size", GUILayout.Width(50f));
        _newWidth = EditorGUILayout.IntField(_newWidth, GUILayout.Width(30f));
        GUILayout.Label("x", GUILayout.Width(10f));
        _newHeight = EditorGUILayout.IntField(_newHeight, GUILayout.Width(30f));
        using (new EditorGUI.DisabledScope(_newWidth == Level.width && _newHeight == Level.height))
        {
            if (GUILayout.Button("Apply", GUILayout.Width(50f)))
            {
                ApplySize();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Moves", GUILayout.Width(50f));
        int moves = EditorGUILayout.IntField(Level.moves, GUILayout.Width(40f));
        EditorGUILayout.EndHorizontal();
        if (moves != Level.moves)
        {
            Change("Change Moves", () => Level.moves = Mathf.Max(1, moves));
        }

        DrawColors();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Stars", "Moves left needed for 2 and 3 stars. Both 0 = default ratios."), GUILayout.Width(50f));
        GUILayout.Label("2★", GUILayout.Width(22f));
        int two = EditorGUILayout.IntField(Level.stars.two, GUILayout.Width(30f));
        GUILayout.Label("3★", GUILayout.Width(22f));
        int three = EditorGUILayout.IntField(Level.stars.three, GUILayout.Width(30f));
        EditorGUILayout.EndHorizontal();
        if (two != Level.stars.two || three != Level.stars.three)
        {
            Change("Change Stars", () =>
            {
                Level.stars.two = Mathf.Max(0, two);
                Level.stars.three = Mathf.Max(0, three);
            });
        }
    }

    private void ApplySize()
    {
        int width = Mathf.Clamp(_newWidth, LevelValidator.MinSize, LevelValidator.MaxSize);
        int height = Mathf.Clamp(_newHeight, LevelValidator.MinSize, LevelValidator.MaxSize);
        Change("Resize Level", () => LevelEditing.Resize(Level, width, height));

        _newWidth = width;
        _newHeight = height;
        GUI.FocusControl(null);
    }

    private void DrawColors()
    {
        List<PieceColor> current = LevelValidator.ParseColors(Level.colors);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Colors", GUILayout.Width(50f));
        EditorGUIUtility.SetIconSize(new Vector2(22f, 22f));

        foreach (PieceColor color in AllColors)
        {
            bool on = current.Contains(color);
            Texture icon = _preview.GetTexture(ColorBrush(color));
            GUIContent content = icon != null
                ? new GUIContent(icon, color.ToString())
                : new GUIContent(color.ToString().Substring(0, 1), color.ToString());

            using (new EditorGUI.DisabledScope(on && current.Count <= 2))
            {
                bool next = GUILayout.Toggle(on, content, "Button", GUILayout.Width(36f), GUILayout.Height(30f));
                if (next != on)
                {
                    List<PieceColor> colors = AllColors.Where(c => c == color ? next : current.Contains(c)).ToList();
                    Change("Change Colors", () => LevelEditing.SetColors(Level, colors));
                }
            }
        }

        EditorGUIUtility.SetIconSize(Vector2.zero);
        EditorGUILayout.EndHorizontal();
    }

    private static ILevelElement ColorBrush(PieceColor color)
    {
        return LevelElements.ByLayer(LevelLayer.Cell).OfType<ColorElement>().FirstOrDefault(e => e.Color == color);
    }

    private void DrawGoals()
    {
        GUILayout.Label("Goals", EditorStyles.boldLabel);

        Dictionary<GoalType, int> totals = LevelEditing.CountAutoGoals(Level);
        LevelGoalData toRemove = null;

        foreach (LevelGoalData goal in Level.goals)
        {
            Enum.TryParse(goal.type, out GoalType type);
            bool isColorGoal = type == GoalType.CollectColor;
            totals.TryGetValue(type, out int total);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(isColorGoal ? $"Collect {goal.color}" : goal.type);
            if (!isColorGoal)
            {
                GUILayout.Label($"/ {total}", EditorStyles.miniLabel, GUILayout.Width(30f));
            }

            int count = EditorGUILayout.IntField(goal.count, GUILayout.Width(36f));
            if (count != goal.count)
            {
                Change("Change Goal", () => goal.count = !isColorGoal && total > 0 ? Mathf.Clamp(count, 1, total) : Mathf.Max(1, count));
            }

            if (isColorGoal)
            {
                if (GUILayout.Button("×", GUILayout.Width(20f)))
                {
                    toRemove = goal;
                }
            }
            else
            {
                GUILayout.Space(24f);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (toRemove != null)
        {
            Change("Remove Goal", () => Level.goals.Remove(toRemove));
        }
        if (Level.goals.Count == 0)
        {
            GUILayout.Label("No goals yet.", EditorStyles.miniLabel);
        }

        DrawAddColorGoal();
    }

    private void DrawAddColorGoal()
    {
        List<PieceColor> free = LevelValidator.ParseColors(Level.colors)
            .Where(c => !Level.goals.Exists(g => g.type == nameof(GoalType.CollectColor) && g.color == c.ToString()))
            .ToList();
        if (free.Count == 0)
        {
            return;
        }

        _newGoalColor = Mathf.Clamp(_newGoalColor, 0, free.Count - 1);

        EditorGUILayout.BeginHorizontal();
        _newGoalColor = EditorGUILayout.Popup(_newGoalColor, free.Select(c => c.ToString()).ToArray(), GUILayout.Width(70f));
        _newGoalCount = Mathf.Max(1, EditorGUILayout.IntField(_newGoalCount, GUILayout.Width(36f)));
        if (GUILayout.Button("+ Color goal"))
        {
            PieceColor color = free[_newGoalColor];
            int count = _newGoalCount;
            Change("Add Goal", () => LevelEditing.AddColorGoal(Level, color, count));
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawMessages()
    {
        if (!string.IsNullOrEmpty(_message))
        {
            EditorGUILayout.HelpBox(_message, MessageType.Warning);
        }

        if (_errors.Count == 0)
        {
            EditorGUILayout.HelpBox("The level is valid.", MessageType.Info);
            return;
        }

        string text = string.Join("\n", _errors.Take(MaxErrorLines));
        if (_errors.Count > MaxErrorLines)
        {
            text += $"\n... and {_errors.Count - MaxErrorLines} more";
        }
        EditorGUILayout.HelpBox(text, MessageType.Error);
    }
    
    private Rect BoardRect(Rect area, out float cell)
    {
        cell = Mathf.Floor(Mathf.Min(
            (area.width - BoardPadding * 2f) / Level.width,
            (area.height - BoardPadding * 2f) / Level.height));
        cell = Mathf.Max(cell, 8f);

        float width = cell * Level.width, height = cell * Level.height;
        return new Rect(area.x + (area.width - width) / 2f, area.y + (area.height - height) / 2f, width, height);
    }

    private static Rect CellRect(Rect board, float cell, int x, int y)
    {
        return new Rect(board.x + x * cell, board.y + y * cell, cell, cell);
    }

    private static Rect Shrink(Rect rect, float amount)
    {
        return new Rect(rect.x + amount, rect.y + amount, rect.width - amount * 2f, rect.height - amount * 2f);
    }

    private bool TryGetCell(Rect board, float cell, Vector2 mouse, out int x, out int y)
    {
        x = Mathf.FloorToInt((mouse.x - board.x) / cell);
        y = Mathf.FloorToInt((mouse.y - board.y) / cell);
        return board.Contains(mouse) && Level.InBounds(x, y);
    }

    private void DrawBoard(Rect board, float cell)
    {
        LevelData level = Level;

        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                Rect rect = CellRect(board, cell, x, y);
                char code = level.At(LevelLayer.Cell, x, y);
                bool hole = code == '#';

                EditorGUI.DrawRect(Shrink(rect, 1f), hole ? HoleColor : ((x + y) % 2 == 0 ? TileA : TileB));
                if (hole)
                {
                    continue;
                }

                DrawCode(LevelLayer.Floor, level.At(LevelLayer.Floor, x, y), rect);
                DrawCode(LevelLayer.Cell, code, Shrink(rect, cell * 0.12f));
            }
        }

        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                char code = level.At(LevelLayer.Overlay, x, y);
                if (code == LevelData.Empty)
                {
                    continue;
                }

                ILevelElement element = LevelElements.Get(LevelLayer.Overlay, code);
                Vector2Int size = element != null ? _preview.GetSize(element) : Vector2Int.one;
                DrawCode(LevelLayer.Overlay, code, new Rect(board.x + x * cell, board.y + y * cell, cell * size.x, cell * size.y));
            }
        }

        if (TryGetCell(board, cell, Event.current.mousePosition, out int hoverX, out int hoverY))
        {
            EditorGUI.DrawRect(CellRect(board, cell, hoverX, hoverY), HoverColor);
        }
    }

    private void DrawCode(LevelLayer layer, char code, Rect rect)
    {
        if (code == LevelData.Empty)
        {
            return;
        }

        ILevelElement element = LevelElements.Get(layer, code);
        Texture texture = element != null ? _preview.GetTexture(element) : null;
        if (texture != null)
        {
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            return;
        }

        if (element == null)
        {
            EditorGUI.DrawRect(rect, UnknownColor);
        }
        _codeStyle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        GUI.Label(rect, code.ToString(), _codeStyle);
    }
    
    private void HandleInput(Rect board, float cell)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseMove)
        {
            Repaint();
            return;
        }
        if (e.type == EventType.MouseUp)
        {
            _lastPainted = (-1, -1);
            return;
        }
        if (e.button != 0 || (e.type != EventType.MouseDown && e.type != EventType.MouseDrag))
        {
            return;
        }
        if (!TryGetCell(board, cell, e.mousePosition, out int x, out int y))
        {
            return;
        }

        if (e.type == EventType.MouseDown)
        {
            Undo.IncrementCurrentGroup();
            _lastPainted = (-1, -1);
        }

        if (_lastPainted != (x, y))
        {
            _lastPainted = (x, y);
            ApplyBrush(x, y);
        }
        e.Use();
    }

    private void ApplyBrush(int x, int y)
    {
        Undo.RecordObject(_asset, _brush != null ? "Paint " + _brush.DisplayName : "Erase");

        bool changed;
        if (_brush == null)
        {
            changed = LevelEditing.Erase(Level, Layer, x, y);
            _message = null;
        }
        else
        {
            changed = LevelEditing.Paint(Level, _brush, x, y, out string reason);
            _message = changed ? null : $"{_brush.DisplayName} at ({x},{y}): {reason}";
        }

        if (changed)
        {
            EditorUtility.SetDirty(_asset);
            Validate();
        }
        Repaint();
    }
    
    private void DrawBot()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Bot test", EditorStyles.boldLabel, GUILayout.Width(60f));

        if (BotRunning)
        {
            int done = Volatile.Read(ref _botDone);
            Rect bar = GUILayoutUtility.GetRect(100f, 18f, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(bar, done / (float)_botTotal, $"Run {done} / {_botTotal}");
            if (GUILayout.Button("Cancel", GUILayout.Width(60f))) CancelBot();
        }
        else
        {
            GUILayout.Label("Runs", GUILayout.Width(34f));
            _botRunCount = EditorGUILayout.IntField(_botRunCount, GUILayout.Width(44f));
            using (new EditorGUI.DisabledScope(_errors.Count > 0))
            {
                if (GUILayout.Button("Run", GUILayout.Width(60f))) RunBot();
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        if (_botResults == null) return;

        LevelBot.Report report = LevelBot.Analyze(_botResults, Level.moves);
        bool stale = BotKey(Level) != _botLevelKey;

        string stars = report.SuggestedThreeStars > 0
            ? $"2★ {report.SuggestedTwoStars} moves left, 3★ {report.SuggestedThreeStars} moves left"
            : "too hard for star thresholds, add moves first";

        string text =
            $"Difficulty: {report.Difficulty}   (win rate {report.WinRate:P0} with {report.MoveLimit} moves)\n" +
            $"Suggested stars: {stars}\n" +
            $"Moves to finish: median {Show(report.MedianMoves)}, {LevelBot.TargetWinRate:P0} of runs within {Show(report.SuggestedMoves)}\n" +
            $"Hardest goal: {report.HardestGoal ?? "-"} (last to finish in {report.HardestGoalShare:P0} of runs)" +
            (report.Unfinished > 0 ? $"\nNever finished: {report.Unfinished} runs" : "") +
            $"\n{report.Runs} runs, {_botSeconds:0.0}s";
        if (stale) text += "\nThe board or goals changed after this test. Run it again.";

        MessageType type = stale ? MessageType.Warning
            : report.Difficulty == LevelDifficulty.TooHard ? MessageType.Error
            : MessageType.None;
        EditorGUILayout.HelpBox(text, type);
        
        if (stale)
        {
            text += "\nThe board or goals changed after this test. Run it again.";
        }
        
        EditorGUILayout.HelpBox(text, stale ? MessageType.Warning : MessageType.None);

        EditorGUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(report.SuggestedMoves < 1 || report.SuggestedMoves == Level.moves))
        {
            if (GUILayout.Button($"Use {Show(report.SuggestedMoves)} moves"))
            {
                Change("Apply Bot Moves", () => Level.moves = report.SuggestedMoves);
            }
        }
        using (new EditorGUI.DisabledScope(report.SuggestedThreeStars < 1))
        {
            if (GUILayout.Button("Use suggested stars"))
            {
                Change("Apply Bot Stars", () =>
                {
                    Level.stars.two = report.SuggestedTwoStars;
                    Level.stars.three = report.SuggestedThreeStars;
                });
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private bool BotRunning => _botTask != null;

    private void RunBot()
    {
        int runs = Mathf.Clamp(_botRunCount, 10, 1000);
        _botRunCount = runs;

        LevelData snapshot = LevelEditorAssets.CopyData(Level);
        _botPendingKey = BotKey(snapshot);
        _botCancel = new CancellationTokenSource();
        CancellationToken token = _botCancel.Token;
        _botDone = 0;
        _botTotal = runs;
        _botTimer = Stopwatch.StartNew();

        _botTask = Task.Run(() =>
        {
            var results = new List<LevelBot.RunResult>(runs);
            for (int seed = 0; seed < runs && !token.IsCancellationRequested; seed++)
            {
                results.Add(LevelBot.Play(snapshot, seed));
                Interlocked.Increment(ref _botDone);
            }
            return results;
        }, token);

        EditorApplication.update += PollBot;
    }

    private void PollBot()
    {
        if (_botTask == null)
        {
            EditorApplication.update -= PollBot;
            return;
        }

        if (!_botTask.IsCompleted)
        {
            Repaint();
            return;
        }

        CancelBot();
        EditorApplication.update -= PollBot;
        if (_botTask.IsFaulted)
        {
            UnityEngine.Debug.LogException(_botTask.Exception);
        }
        else if (!_botTask.IsCanceled && _botTask.Result.Count > 0)
        {
            _botResults = _botTask.Result;
            _botLevelKey = _botPendingKey;
            _botSeconds = _botTimer.Elapsed.TotalSeconds;
            if (_tuneTarget.HasValue)
            {
                ApplyTuning(_tuneTarget.Value);
            }
        }

        _botTask = null;
        _tuneTarget = null;
        _botCancel?.Dispose();
        _botCancel = null;
        Repaint();
    }

    private void CancelBot()
    {
        _botCancel?.Cancel();
    }

    private static string BotKey(LevelData level)
    {
        LevelData copy = LevelEditorAssets.CopyData(level);
        copy.moves = 0;
        copy.stars = new LevelStars();
        return LevelJson.ToJson(copy);
    }
    
    private static string Show(int value)
    {
        return value < 0 ? "-" : value.ToString();
    }
    
    public static void OpenAndTune(LevelAsset asset, LevelDifficulty target)
    {
        var window = GetWindow<LevelEditorWindow>("Level Editor");
        window.LoadAsset(asset);
        window._tuneTarget = target;
        window.RunBot();
        window.Focus();
    }
    
    private void ApplyTuning(LevelDifficulty target)
    {
        int moves = LevelBot.MovesFor(_botResults, target);
        if (moves < 1)
        {
            _message = $"The bot couldn't reach {target} even with many moves. Try fewer obstacles or a smaller goal.";
            return;
        }

        LevelBot.Report report = LevelBot.Analyze(_botResults, moves);
        Change("Bot Tuning", () =>
        {
            Level.moves = moves;
            Level.stars.two = report.SuggestedTwoStars;
            Level.stars.three = report.SuggestedThreeStars;
        });
        AssetDatabase.SaveAssets();
    }
}