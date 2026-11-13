using UnityEngine;

[CreateAssetMenu(fileName = "AnimationSettings", menuName = "Match3/AnimationSettings")]
public class AnimationSettings : ScriptableObject
{
    [Header("Board")]
    public float PopDuration = 0.1f;
    public float SwapDuration = 0.15f;
    public float FallSecondsPerCell = 0.08f;
    public float FallDelay = 0.06f;
    public float MergeDuration = 0.16f;

    [Header("Toys")]
    public float CollectJumpDuration = 0.3f;
    public float CollectFlyDuration = 0.55f;

    [Header("Goal flights")]
    public float IconFlyDuration = 0.6f;
    public float SparkFlyDuration = 0.45f;
    public float ArcHeight = 1.5f;
    public float ArcSpread = 1.5f;
}