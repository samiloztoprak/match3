using System.Text;
using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private TMP_Text _goalsText;

    public void SetMoves(int movesRemaining)
    {
        _movesText.text = movesRemaining.ToString();
    }

    public void SetGoals(GoalTracker tracker)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < tracker.Goals.Count; i++)
        {
            LevelGoal goal = tracker.Goals[i];
            int remaining = tracker.GetRemaining(i);

            sb.Append(GoalLabel(goal));
            sb.Append(": ");
            sb.Append(remaining);
            sb.Append("   ");
        }
        _goalsText.text = sb.ToString();
    }

    private string GoalLabel(LevelGoal goal)
    {
        switch (goal.Type)
        {
            case GoalType.CollectColor: return goal.Color.ToString();
            case GoalType.ClearBox: return "Box";
            case GoalType.ClearLock: return "Lock";
            case GoalType.ClearGrass: return "Grass";
            case GoalType.ClearCage: return "Cage";
            default: return goal.Type.ToString();
        }
    }
}