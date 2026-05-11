using System;
using System;
using UnityEngine;

public enum CollisionSide
{
    None,
    Left,
    Right,
    Top,
    Bottom
}

public enum HazardType
{
    None,
    STEAM
}

[Flags]
public enum DamagePropertyFlags
{
    None = 0
}

public class DamageReference : ScriptableObject
{
    [SerializeField] private int value = 1;
    public int Value => value;
}

public class HealthManager : MonoBehaviour
{
    public event Action TookDamage;

    public void RaiseTookDamage()
    {
        TookDamage?.Invoke();
    }
}

public class Recoil : MonoBehaviour
{
    public void RecoilByDirection(int attackDirection, float magnitude)
    {
    }
}

public class NonBouncer : MonoBehaviour
{
}

public static class TerrainThunkUtils
{
    public enum SlashDirection
    {
        None
    }

    public struct TerrainThunkConditionArgs
    {
        public int RecoilDirection;
        public Vector3 ThunkPos;
    }

    public delegate bool TerrainThunkConditionDelegate(TerrainThunkConditionArgs args);

    public static void GenerateTerrainThunk(Collision2D collision, ContactPoint2D[] contacts, SlashDirection slashDirection, Vector3 origin, out int attackDirection, out int collisionType, TerrainThunkConditionDelegate condition)
    {
        attackDirection = 0;
        collisionType = 0;
    }
}

public static class DebugDrawColliderRuntime
{
    public enum ColorType
    {
        Danger
    }

    public static void AddOrUpdate(GameObject gameObject, ColorType type, bool enabled)
    {
    }
}
