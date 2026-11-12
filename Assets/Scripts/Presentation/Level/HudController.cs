using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private Transform _goalSlotParent;
    [SerializeField] private GoalSlotView _goalSlotPrefab;
    [SerializeField] private GoalIconConfig _goalIcons;

    private readonly List<GoalSlotView> _slots = new List<GoalSlotView>();

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
            _goalIcons.TryGet(goal.Type, goal.Color, out Sprite icon, out _, out _);

            GoalSlotView slot = Instantiate(_goalSlotPrefab, _goalSlotParent);
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