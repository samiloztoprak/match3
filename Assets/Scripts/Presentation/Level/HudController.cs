using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Transform _goalSlotParent;
    [SerializeField] private GameAssets _assets;

    private readonly List<GoalSlotView> _slots = new List<GoalSlotView>();
    
    private void OnValidate()
    {
        if (_movesText == null) _movesText = AutoRef.Child<TMP_Text>(this, "ui_text_moves_value");
        if (_levelText == null) _levelText = AutoRef.Child<TMP_Text>(this, "ui_text_level_value");
        if (_goalSlotParent == null) _goalSlotParent = AutoRef.Child<Transform>(this, "ui_grid_goal_slots");
#if UNITY_EDITOR
        if (_assets == null) _assets = AutoRef.Asset<GameAssets>();
#endif
    }
    
    public void SetLevel(int levelNumber)
    {
        _levelText.text = "LEVEL " + levelNumber;
    }
    
    public void SetMoves(int movesRemaining)
    {
        _movesText.text = movesRemaining.ToString();
    }

    public void BuildGoals(GoalTracker tracker)
    {
        foreach (GoalSlotView slot in _slots)
        {
            Destroy(slot.gameObject);
        }
        _slots.Clear();

        for (int i = 0; i < tracker.Goals.Count; i++)
        {
            LevelGoal goal = tracker.Goals[i];
            _assets.GoalIcons.TryGet(goal.Type, goal.Color, out Sprite icon, out _, out _);

            GoalSlotView slot = Instantiate(_assets.GoalSlot, _goalSlotParent);
            slot.Setup(goal, icon, tracker.GetRemaining(i));
            _slots.Add(slot);
        }
    }

    public GoalSlotView FindSlot(Piece piece)
    {
        foreach (GoalSlotView slot in _slots)
        {
            if (slot.HasPending && slot.Matches(piece))
            {
                return slot;
            }
        }
        return null;
    }

    public void SyncGoals(GoalTracker tracker)
    {
        for (int i = 0; i < _slots.Count && i < tracker.Goals.Count; i++)
        {
            _slots[i].SyncTo(tracker.GetRemaining(i));
        }
    }
}