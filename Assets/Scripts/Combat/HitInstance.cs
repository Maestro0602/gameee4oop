using UnityEngine;

public enum HitType { Physical, Spell, Hazard }

public struct HitInstance
{
    public GameObject Source;
    public int DamageAmount;
    public float KnockbackForce;
    public float DirectionAngle;
    public HitType AttackType;
    public bool IgnoreInvincibility;

    public HitInstance(GameObject source, int damage, float force, float angle)
    {
        Source = source;
        DamageAmount = damage;
        KnockbackForce = force;
        DirectionAngle = angle;
        AttackType = HitType.Physical;
        IgnoreInvincibility = false;
    }
}
