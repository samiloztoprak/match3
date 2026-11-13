using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LosePopup : PopupView
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Button _extraMovesButton;
    [SerializeField] private CanvasGroup _extraMovesGroup;
    [SerializeField] private TMP_Text _extraMovesText;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Transform _remainingGoalsParent;
    [SerializeField] private GameAssets _assets;

    private Action _onExtraMoves;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (_levelText == null) _levelText = AutoRef.Child<TMP_Text>(this, "ui_text_level_value");
        if (_extraMovesButton == null) _extraMovesButton = AutoRef.Child<Button>(this, "ui_button_extra_moves");
        if (_extraMovesGroup == null && _extraMovesButton != null) _extraMovesGroup = _extraMovesButton.GetComponent<CanvasGroup>();
        if (_extraMovesText == null) _extraMovesText = AutoRef.Child<TMP_Text>(this, "ui_text_extra_moves_value");
        if (_retryButton == null) _retryButton = AutoRef.Child<Button>(this, "ui_button_retry");
        if (_remainingGoalsParent == null) _remainingGoalsParent = AutoRef.Child<Transform>(this, "ui_grid_remaining_goals");
#if UNITY_EDITOR
        if (_assets == null) _assets = AutoRef.Asset<GameAssets>();
#endif
    }

    private void OnEnable()
    {
        _extraMovesButton.onClick.AddListener(OnExtraMoves);
        _retryButton.onClick.AddListener(OnRetry);
    }

    private void OnDisable()
    {
        _extraMovesButton.onClick.RemoveListener(OnExtraMoves);
        _retryButton.onClick.RemoveListener(OnRetry);
    }

    public void Setup(int levelNumber, int extraMoves, bool canContinue, Action onExtraMoves, GoalTracker tracker)
    {
        _levelText.text = "LEVEL " + levelNumber;
        _extraMovesText.text = "+" + extraMoves + " MOVES";
        _onExtraMoves = onExtraMoves;

        _extraMovesButton.interactable = canContinue;
        _extraMovesGroup.alpha = canContinue ? 1f : 0.5f;

        BuildRemainingGoals(tracker);
    }

    private void BuildRemainingGoals(GoalTracker tracker)
    {
        foreach (Transform child in _remainingGoalsParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < tracker.Goals.Count; i++)
        {
            int remaining = tracker.GetRemaining(i);
            if (remaining <= 0) continue;

            LevelGoal goal = tracker.Goals[i];
            _assets.GoalIcons.TryGet(goal.Type, goal.Color, out Sprite icon, out _, out _);
            GoalSlotView slot = Instantiate(_assets.GoalSlot, _remainingGoalsParent);
            slot.Setup(goal, icon, remaining);
        }
    }

    private void OnExtraMoves()
    {
        Action callback = _onExtraMoves;
        _onExtraMoves = null;
        Close(callback);
    }

    private void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}