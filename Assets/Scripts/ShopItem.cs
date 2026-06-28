using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class ShopItem : MonoBehaviour
{
    [Header("Shop Settings")]
    [Tooltip("The name of the item (for UI/debug).")]
    public string itemName = "Mystery Item";
    
    [Tooltip("How much does this item cost?")]
    public int cost = 50;
    
    [Tooltip("Which currency is used?")]
    public CurrencyType currencyType = CurrencyType.Money;

    [Header("Events")]
    [Tooltip("What happens when the player buys this item? (e.g. give an item, unlock a skill)")]
    public UnityEvent OnPurchased;
    
    [Tooltip("What happens if they don't have enough money? (e.g. play error sound)")]
    public UnityEvent OnFailedPurchase;

    private bool playerIsNear = false;

    private void Awake()
    {
        // Ensure the collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (playerIsNear)
        {
            // Defaulting to "Up Arrow" or "W" for interaction
            bool interactPressed = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
            
            if (interactPressed)
            {
                TryBuyItem();
            }
        }
    }

    private void TryBuyItem()
    {
        int currentCurrency = CurrencyManager.GetCurrencyAmount(currencyType);

        if (currentCurrency >= cost)
        {
            // Player has enough money! Deduct it.
            if (currencyType == CurrencyType.Money)
            {
                CurrencyManager.TakeRocksAndPebbles(cost);
            }
            else
            {
                // If you ever implement taking shards, do it here
                // e.g. PlayerData.instance.ShellShards -= cost;
            }

            Debug.Log($"[ShopItem] Bought {itemName} for {cost} {currencyType}!");
            
            // Trigger the success event
            OnPurchased?.Invoke();
            
            // Optionally destroy the item so it can't be bought twice
            // Destroy(gameObject);
        }
        else
        {
            // Not enough money
            Debug.Log($"[ShopItem] Not enough {currencyType} for {itemName}. You have {currentCurrency}, need {cost}.");
            OnFailedPurchase?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.GetComponent<HeroController>() != null)
        {
            playerIsNear = true;
            Debug.Log($"[ShopItem] Player approached {itemName}. Press UP or W to buy.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.GetComponent<HeroController>() != null)
        {
            playerIsNear = false;
        }
    }
}
