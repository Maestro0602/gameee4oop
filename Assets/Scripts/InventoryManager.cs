using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Inventory Data")]
    // A dictionary to store all our items and their counts
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    [Header("Potions")]
    public int maxHealthPotions = 99;

    private void Awake()
    {
        // Singleton pattern so other scripts can access the inventory easily
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------------------------------------------
    // General Inventory Functions
    // -------------------------------------------------------------

    public void AddItem(string itemName, int amount)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] += amount;
        }
        else
        {
            inventory.Add(itemName, amount);
        }
        Debug.Log($"[Inventory] Added {amount} {itemName}(s). Total: {inventory[itemName]}");
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] -= amount;
            if (inventory[itemName] < 0) inventory[itemName] = 0;
            Debug.Log($"[Inventory] Removed {amount} {itemName}(s). Remaining: {inventory[itemName]}");
        }
    }

    public int GetItemCount(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName];
        }
        return 0;
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        return GetItemCount(itemName) >= amount;
    }

    // -------------------------------------------------------------
    // Potion Specific Helper Functions (For UnityEvents in Shops)
    // -------------------------------------------------------------

    /// <summary>
    /// Call this from your ShopItem UnityEvent to give the player a Health Potion.
    /// </summary>
    public void AddHealthPotion(int amount)
    {
        int currentPotions = GetItemCount("Health Potion");
        
        if (currentPotions + amount > maxHealthPotions)
        {
            // If buying this goes over the cap, just set it to the cap
            int amountToAdd = maxHealthPotions - currentPotions;
            AddItem("Health Potion", amountToAdd);
            Debug.Log("[Inventory] Reached maximum health potions!");
        }
        else
        {
            AddItem("Health Potion", amount);
        }
    }

    /// <summary>
    /// Consumes a health potion if the player has one, and heals them.
    /// </summary>
    public bool TryConsumeHealthPotion()
    {
        if (HasItem("Health Potion", 1))
        {
            // Only heal if we aren't already at max health
            if (PlayerData.instance.health < PlayerData.instance.maxHealth)
            {
                RemoveItem("Health Potion", 1);
                PlayerData.instance.AddHealth(1);
                Debug.Log($"[Inventory] Consumed 1 Health Potion! HP is now {PlayerData.instance.health}");

#if PLAYMAKER
                // Notify the Hero's PlayMaker FSM so the UI HP Bar updates!
                HeroController hero = FindFirstObjectByType<HeroController>();
                if (hero != null)
                {
                    PlayMakerFSM[] fsms = hero.GetComponents<PlayMakerFSM>();
                    foreach (var fsm in fsms)
                    {
                        fsm.SendEvent("HealPlayer");
                    }
                }
#endif
                return true; // Successfully consumed
            }
            else
            {
                Debug.Log("[Inventory] You are already at full health! Potion not consumed.");
                return false;
            }
        }
        else
        {
            Debug.Log("[Inventory] You don't have any Health Potions to drink!");
            return false;
        }
    }
}
