using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LevelGeneratorWindow : EditorWindow
{
    private static readonly LevelLayer[] Layers = { LevelLayer.Cell, LevelLayer.Floor, LevelLayer.Overlay };
    private static readonly LevelDifficulty[] Targets =
        { LevelDifficulty.Easy, LevelDifficulty.Normal, LevelDifficulty.Hard, LevelDifficulty.VeryHard };

    [SerializeField] private LevelGeneratorSettings _settings = new LevelGeneratorSettings();
    [SerializeField] private LevelDifficulty _target = LevelDifficulty.Normal;
    [SerializeField] private bool _newSeedEachTime = true;
    [SerializeField] private int _seed = 1;

    private LevelEditorPreview _preview;
    private Vector2 _scroll;
    private string _message;

    [MenuItem("Match3/Level Generator")]
    public static void Open() => GetWindow<LevelGeneratorWindow>(true, "Level Generator");

    private void OnEnable()
    {
        _preview = new LevelEditorPreview();
        SyncPieceList();
    }

    private void SyncPieceList()
    {
        foreach (LevelLayer layer in Layers)
        {
            foreach (ILevelElement element in LevelElements.ByLayer(layer))
            {
                if (element is ColorElement) continue;
                string code = element.Code.ToString();
                if (!_settings.pieces.Exists(p => p.layer == layer && p.code == code))
                {
                    _settings.pieces.Add(new GeneratorPiece { layer = layer, code = code });
                }
            }
        }
    }

    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 120f;
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        GUILayout.Label("Board", EditorStyles.boldLabel);
        _settings.width = EditorGUILayout.IntSlider("Width", _settings.width, LevelValidator.MinSize, LevelValidator.MaxSize);
        _settings.height = EditorGUILayout.IntSlider("Height", _settings.height, LevelValidator.MinSize, LevelValidator.MaxSize);
        _settings.colorCount = EditorGUILayout.IntSlider("Colors", _settings.colorCount, 2, 4);
        _settings.mirror = EditorGUILayout.Toggle("Mirror (symmetric)", _settings.mirror);

        GUILayout.Space(6f);
        GUILayout.Label("Pieces", EditorStyles.boldLabel);
        EditorGUIUtility.SetIconSize(new Vector2(20f, 20f));
        foreach (GeneratorPiece piece in _settings.pieces)
        {
            ILevelElement element = string.IsNullOrEmpty(piece.code) ? null : LevelElements.Get(piece.layer, piece.code[0]);
            if (element == null) continue;
            var label = new GUIContent(" " + element.DisplayName, _preview.GetTexture(element));
            piece.count = EditorGUILayout.IntSlider(label, piece.count, 0, 20);
        }
        EditorGUIUtility.SetIconSize(Vector2.zero);

        GUILayout.Space(6f);
        GUILayout.Label("Goal and difficulty", EditorStyles.boldLabel);
        _settings.colorGoalCount = EditorGUILayout.IntSlider(
            new GUIContent("Color goal", "Collect N pieces of a random level color. 0 = none."), _settings.colorGoalCount, 0, 60);
        int target = Mathf.Max(0, Array.IndexOf(Targets, _target));
        _target = Targets[EditorGUILayout.Popup("Difficulty", target, Targets.Select(d => d.ToString()).ToArray())];

        GUILayout.Space(6f);
        GUILayout.Label("Seed", EditorStyles.boldLabel);
        _newSeedEachTime = EditorGUILayout.Toggle("New seed each time", _newSeedEachTime);
        using (new EditorGUI.DisabledScope(_newSeedEachTime))
        {
            _seed = EditorGUILayout.IntField("Seed", _seed);
        }

        GUILayout.Space(10f);
        if (GUILayout.Button("Generate new level", GUILayout.Height(32f)))
        {
            Generate();
        }
        if (!string.IsNullOrEmpty(_message))
        {
            EditorGUILayout.HelpBox(_message, MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        if (_newSeedEachTime) _seed = UnityEngine.Random.Range(1, 1000000);

        LevelData level = LevelGenerator.Generate(_settings, _seed);
        if (level.goals.Count == 0)
        {
            _message = "The level has no goals. Add a goal piece (box, toy, lock, grass) or a color goal.";
            return;
        }

        var errors = LevelValidator.Validate(level);
        if (errors.Count > 0)
        {
            _message = "The generated level isn't valid:\n- " + string.Join("\n- ", errors);
            return;
        }

        LevelAsset asset = LevelEditorAssets.Create(level);
        LevelEditorWindow.OpenAndTune(asset, _target);
        _message = $"Created {asset.name} with seed {_seed}. The bot is setting moves and stars for {_target}.";
        GUIUtility.ExitGUI();
    }
}