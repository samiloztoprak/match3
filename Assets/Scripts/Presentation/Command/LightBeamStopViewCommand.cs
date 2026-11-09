using System.Collections.Generic;
using UnityEngine;

public class LightBeamStopViewCommand : IViewCommand
{
    private readonly TurnPlayer _player;
    private readonly List<LightBeamView> _activeBeams;

    public float Duration => 0f;

    public LightBeamStopViewCommand(TurnPlayer player, List<LightBeamView> activeBeams)
    {
        _player = player;
        _activeBeams = activeBeams;
    }

    public void Execute()
    {
        foreach (var beam in _activeBeams)
        {
            beam.Stop();
            _player.ReleaseLightBeam(beam);
        }
        _activeBeams.Clear();
    }
}
