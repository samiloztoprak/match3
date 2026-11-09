using UnityEngine;

[CreateAssetMenu(fileName = "PropellerAnimationConfig", menuName = "Match3/PropellerAnimationConfig")]
public class PropellerAnimationConfig : ScriptableObject
{
    [Header("Flight")]
    public float PeakScale = 1.4f;
    public float WeaveFactor = 0.25f;
    public float MinWeaveAmount = 0.3f;
    public float ArcHeightFactor = 0.3f;
    public float MinArcHeight = 0.5f;

    [Header("Impact")]
    public float ImpactFlashDuration = 0.2f;

    [Header("Rendering")]
    public int FlightSortingOrder = 10;
}
