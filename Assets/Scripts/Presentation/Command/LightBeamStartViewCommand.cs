using System.Collections.Generic;
using UnityEngine;

public class LightBeamStartViewCommand : IViewCommand
{
    private readonly Vector3 _from;
    private readonly Vector3 _to;
    private readonly TurnPlayer _player;
    private readonly List<LightBeamView> _activeBeams;
    
    public float Duration => 0f;

    public LightBeamStartViewCommand(Vector3 from, Vector3 to, TurnPlayer player, List<LightBeamView> activeBeams)
    {
        _from = from;
        _to = to;
        _player = player;
        _activeBeams = activeBeams;
    }

    public void Execute()
    {
        LightBeamView beam = _player.SpawnLightBeam(_from);
        beam.Play(_from, _to);
        _activeBeams.Add(beam);
    }
}
