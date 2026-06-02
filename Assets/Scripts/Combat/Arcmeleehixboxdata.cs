using UnityEngine;

[CreateAssetMenu(fileName = "NewArcMeleeHitboxData", menuName = "Combat/Arc Melee Hitbox Data")]
public class ArcMeleeHitboxData : ScriptableObject
{
    [Header("Arc Angles (Degrees)")]
    public float normalArc = 120f;
    public float upwardArc = 90f;
    public float downwardArc = 90f;

    [Header("Hitbox Properties")]
    public float radius = 2f;
    public int segments = 12;
    public float activeDuration = 0.15f;

    [Header("Combo Settings")]
    public float comboTimeWindow = 0.5f;
    public float attackCooldown = 0.25f;
}