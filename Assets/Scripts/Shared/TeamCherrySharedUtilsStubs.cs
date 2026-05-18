namespace TeamCherry.SharedUtils
{
    // Stub interface to satisfy references in generated assets.
    public interface IIncludeVariableExtensions
    {
    }

    public interface IInitialisable
    {
        bool OnAwake();
        bool OnStart();
        UnityEngine.GameObject get_gameObject();
    }
}
