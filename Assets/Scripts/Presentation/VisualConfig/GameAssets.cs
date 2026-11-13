using UnityEngine;

[CreateAssetMenu(fileName = "GameAssets", menuName = "Match3/GameAssets")]
public class GameAssets : ScriptableObject
{
    [Header("Board")]
    public PieceView PiecePrefab;
    public SpriteRenderer CellTilePrefab;

    [Header("Configs")]
    public PieceVisualConfig PieceVisuals;
    public BreakEffectConfig BreakEffects;
    public PropellerAnimationConfig PropellerAnimation;
    public GoalIconConfig GoalIcons;

    [Header("Activation views")]
    public RocketActivationView RocketActivation;
    public ExplosionView TntExplosion;
    public LightBeamView LightBeam;
    public PropellerActivationView PropellerActivation;

    [Header("Boosters")]
    public SpriteRenderer Hammer;
    public SpriteRenderer HammerImpact;

    [Header("Goals")]
    public GoalSlotView GoalSlot;
    public SpriteRenderer GoalFlyIcon;
    public GoalSpark GoalSpark;
}