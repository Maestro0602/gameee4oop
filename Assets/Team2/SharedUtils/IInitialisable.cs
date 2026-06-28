namespace Team2.SharedUtils
{
    public interface IInitialisable
    {
        bool OnAwake();
        bool OnStart();
        UnityEngine.GameObject get_gameObject();
    }
}
