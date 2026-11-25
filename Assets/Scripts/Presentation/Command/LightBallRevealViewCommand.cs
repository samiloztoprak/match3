using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LightBallRevealViewCommand : IViewCommand
{

    private readonly int _pieceId;
    private readonly TurnPlayer _player;
    private readonly Vector3 _origin;
    private readonly List<Vector3> _targets;
    private readonly List<LightBeamView> _activeBeams = new List<LightBeamView>();
    
    public float Duration => ActivationTiming.LightBallTotalMs / 1000f;
    private static float RevealWindowMs => ActivationTiming.LightBallTotalMs - ActivationTiming.LightBallPulseMs;
    
    public LightBallRevealViewCommand(int pieceId, TurnPlayer player, Vector3 origin, List<Vector3> targets)
    {
        _pieceId = pieceId;
        _player = player;
        _origin = origin;
        _targets = targets;
    }
    
    public void Execute()
    {
        int count = _targets.Count;
        float available = Mathf.Max(0f, RevealWindowMs - ActivationTiming.LightBallStartMs);
        float stepMs = count > 0 ? Mathf.Min(ActivationTiming.LightBallStepMs, available / count) : 0f;

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < count; i++)
        {
            Vector3 to = _targets[i];
            float at = (ActivationTiming.LightBallStartMs + stepMs * i) / 1000f;
            seq.InsertCallback(at, () => new LightBeamStartViewCommand(_origin, to, _player, _activeBeams).Execute());
        }

        float pulseDuration = ActivationTiming.LightBallPulseMs / 1000f;
        float revealEnd = RevealWindowMs / 1000f;

        seq.InsertCallback(revealEnd, () => new LightBallPulseViewCommand(_pieceId, _player, pulseDuration).Execute());
        seq.InsertCallback(revealEnd + pulseDuration, () => new LightBeamStopViewCommand(_player, _activeBeams).Execute());
    }
}
