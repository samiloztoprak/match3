#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelDebugger : MonoBehaviour
{
    [SerializeField] private BoardController _board;

    [Tooltip("-1 = closed. 0 = Level 1, 11 = Level 12 ...")]
    [SerializeField] private int _startLevelIndex = -1;

    [SerializeField] private int _extraMovesAmount = 5;
    [SerializeField] private bool _showOverlay = true;

    private static bool _startApplied;

    private void Awake()
    {
        if (!_startApplied && _startLevelIndex >= 0)
        {
            LevelProgress.CurrentIndex = _startLevelIndex;
        }
        _startApplied = true;
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rightBracketKey.wasPressedThisFrame) LoadLevel(LevelProgress.CurrentIndex + 1);
        if (kb.leftBracketKey.wasPressedThisFrame)  LoadLevel(LevelProgress.CurrentIndex - 1);
        if (kb.rKey.wasPressedThisFrame)            LoadLevel(LevelProgress.CurrentIndex);
        if (kb.mKey.wasPressedThisFrame && _board != null) _board.DebugAddMoves(_extraMovesAmount);
    }

    private void LoadLevel(int index)
    {
        int count = _board != null ? _board.LevelCount : int.MaxValue;
        LevelProgress.CurrentIndex = Mathf.Clamp(index, 0, count - 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        if (!_showOverlay || _board == null) return;
        int current = Mathf.Clamp(LevelProgress.CurrentIndex, 0, _board.LevelCount - 1) + 1;
        GUI.Label(new Rect(10, 10, 400, 25), $"Level {current} / {_board.LevelCount}   [ ] level   R restart   M +{_extraMovesAmount} moves");
    }

    [ContextMenu("Reset Level Progress")]
    private void ResetProgress()
    {
        PlayerPrefs.DeleteKey("CurrentLevelIndex");
        PlayerPrefs.Save();
        Debug.Log("Level progress reset");
    }

    private void OnApplicationQuit()
    {
        _startApplied = false;
    }
}
#endif