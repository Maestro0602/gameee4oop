using UnityEngine;

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
