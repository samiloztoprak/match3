using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameAssets", menuName = "Match3/GameAssets")]
public class GameAssets : ScriptableObject
{
    [Header("Mascot")]
    public MascotConfig Mascot;
    
    [Header("Board")]
    public PieceView PiecePrefab;
    public CellTile CellTilePrefab;

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
    
    [Header("Levels")]
    [SerializeField, FormerlySerializedAs("Levels")] 
    private LevelCatalog _levelCatalog;
    
    private ILevelSource _remoteSource;
    public ILevelSource Levels => _remoteSource ??= new RemoteAwareLevelSource(_levelCatalog);
}