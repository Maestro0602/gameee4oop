# Architecture Overview: Health, UI, and Economy Systems

## 1. Player HP System
The player's health is strictly tracked in the `PlayerData` singleton (which acts as the single source of truth for save data), while the `HeroController` handles the game logic, physics, and states when taking damage.

```csharp
// Inside HeroController.cs
public void TakeHealth(int amount)
{
    // 1. Tell PlayerData to reduce the actual health/blue health values
    this.playerData.TakeHealth(amount, this.IsInLifebloodState, true);

    // 2. Play animations, give invulnerability, handle hitstop, etc.
    this.HeroDamaged(); 
}

public void AddHealth(int amount)
{
    this.playerData.AddHealth(amount);

    // Broadcast event across the game to update UI and logic
    EventRegister.SendEvent(EventRegisterEvents.HeroHealed, null); 
}
```

## 2. Enemy/Mob HP System (HealthManager)
Enemies don't use `HeroController`. Instead, they have a component attached to their root object called `HealthManager`. The player's hitboxes (`DamageEnemies.cs` or `NailSlash.cs`) create a `HitInstance` struct containing damage info, which is passed to the enemy's `HealthManager`.

```csharp
// Example of how the Hero interacts with Mob Health
public void NailHitEnemy(HealthManager enemyHealth, HitInstance hitInstance)
{
    // Ignore hits if the enemy is invincible or matches specific flags
    if (enemyHealth.ShouldIgnore(HealthManager.IgnoreFlags.RageHeal))
        return;

    // HealthManager internally does: 
    // this.hp -= hitInstance.DamageAmount;
    // if (this.hp <= 0) this.Die();

    // Hero logic gets notified to give rewards upon hitting the mob:
    this.hunterUpgState.CurrentMeterHits++;
}
```

## 3. The UI Connection (Event Driven)
Rather than the `HeroController` talking directly to the `UIManager` or Health UI, this codebase uses an **Event-Driven Architecture** (`EventRegister`). This decouples the game logic from the visual UI layer.

```csharp
// Inside HeroController.cs Taking Damage
private void DoSpecialDamage(int damageAmount, ...)
{
    this.playerData.TakeHealth(damageAmount, this.IsInLifebloodState, canDie);

    // Broadcasts an event out to all listeners. 
    // The UIManager and graphical GameCameras listen to this to shatter health masks on screen.
    EventRegister.SendEvent(EventRegisterEvents.HealthUpdate, null);

    if (this.playerData.health == 0)
    {
        // Broadcasts Death to the UI and Game Manager
        EventRegister.SendEvent(EventRegisterEvents.HeroDeath, null);
    }
}
```

## 4. Money System (Geo & Shards)
Money uses the heavily abstracted `CurrencyManager` class which handles the different types of currencies (like Geo or Shards), triggers the UI counters on screen, and updates the `PlayerData`.

```csharp
// Wrappers inside HeroController.cs that talk to the CurrencyManager
public void AddCurrency(int amount, CurrencyType type, bool showCounter = true)
{
    // Adds Geo/Shards to PlayerData and tells the UI to pop up the counter
    CurrencyManager.ChangeCurrency(amount, type, showCounter);
}

public void TakeGeo(int amount)
{
    CurrencyManager.TakeGeo(amount);
}

public int GetCurrencyAmount(CurrencyType type)
{
    // Checks how much money the player has right now
    return CurrencyManager.GetCurrencyAmount(type);
}
```

## 5. Shop System (Conceptual)
The shop components query the `CurrencyManager` we saw above to verify if a player can afford an item. If so, it subtracts the funds and unlocks the item in `PlayerData`.

```csharp
// A typical ShopMenu implementation interacting with the systems above
public class ShopMenu : MonoBehaviour
{
    public void TryBuyItem(ShopItem item)
    {
        // 1. Check if the player has enough currency
        if (CurrencyManager.GetCurrencyAmount(item.currencyType) >= item.cost)
        {
            // 2. Take the money (updates UI automatically)
            CurrencyManager.ChangeCurrency(-item.cost, item.currencyType, true);

            // 3. Give the player the item in PlayerData
            PlayerData.instance.SetBool(item.playerDataBoolName, true);

            // 4. Optionally heal/reward the player
            if (item.isHealthUpgrade)
            {
               HeroController.instance.AddToMaxHealth(1);
            }

            PlayPurchaseAudio();
        }
        else 
        {
            // Not enough money
            PlayRejectAudio(); 
        }
    }
}
```