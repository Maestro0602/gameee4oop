public interface ICombatToolAbility
{
    int ResourceCost { get; }
    bool TryUse(PlayerController2D controller);
}
