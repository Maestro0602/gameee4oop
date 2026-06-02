using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    public int health = 5;

    private void Awake()
    {
        instance = this;
    }

    public void TakeHealth(int amount, bool isLifeblood, bool canDie)
    {
        health -= amount;
        if (health < 0) health = 0;
    }

    public void AddHealth(int amount)
    {
        health += amount;
    }

    public void SetBool(string name, bool value)
    {
    }
}

public enum EventRegisterEvents
{
    HeroHealed,
    HealthUpdate,
    HeroDeath
}

public static class EventRegister
{
    public static void SendEvent(EventRegisterEvents eventType, object arg)
    {
    }
}

public enum CurrencyType
{
    Geo,
    Shards
}

public static class CurrencyManager
{
    public static void ChangeCurrency(int amount, CurrencyType type, bool showCounter = true)
    {
    }

    public static void TakeGeo(int amount)
    {
    }

    public static int GetCurrencyAmount(CurrencyType type)
    {
        return 0;
    }
}
