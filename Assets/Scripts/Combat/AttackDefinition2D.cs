using UnityEngine;

using UnityEngine;

public enum AttackKind2D
{
    Ground,
    Air,
    Downward,
    Special
}

[CreateAssetMenu(menuName = "Combat/Attack Definition 2D", fileName = "AttackDefinition2D")]
public class AttackDefinition2D : ScriptableObject
{
    [Header("Identity")]
    public string attackId = "basic_slash";
    public AttackKind2D attackKind = AttackKind2D.Ground;

    [Header("Frame Timings")]
    [Min(0f)] public float startupTime = 0.05f;
    [Min(0f)] public float activeTime = 0.08f;
    [Min(0f)] public float recoveryTime = 0.12f;
    [Min(0f)] public float cancelWindowStart = 0.04f;
    [Min(0f)] public float cancelWindowEnd = 0.12f;
    [Min(0f)] public float comboWindow = 0.2f;

    [Header("Movement Integration")]
    public Vector2 velocityEffect = new Vector2(2.2f, 0f);
    [Range(0f, 1.5f)] public float movementControlMultiplier = 1f;

    [Header("Hit")]
    [Min(0f)] public float damage = 3f;
    public Vector2 hitboxOffset = new Vector2(0.2f, -2.1f);
    public Vector2 hitboxSize = new Vector2(0.1f, 1f);
    public LayerMask hittableLayers;

    [Header("Air / Utility")]
    public bool bounceOnHit;
    public float bounceVelocity = 12f;
    public bool resetAerialOptionsOnHit = true;

    [Header("Resource")]
    [Min(0)] public int resourceCost;
    [Min(0f)] public float resourceGainOnHit = 12f;
}
