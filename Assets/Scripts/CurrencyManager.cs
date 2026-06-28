using UnityEngine;

public static class CurrencyManager
{
    public static PlayerData playerData
    {
        get
        {
            if (PlayerData.instance == null)
            {
                Debug.LogWarning("PlayerData instance is null. Attempting to find one.");
                PlayerData.instance = Object.FindFirstObjectByType<PlayerData>();
            }
            return PlayerData.instance;
        }
    }

    public static void AddCurrency(int amount, CurrencyType type, bool showCounter = true)
    {
        ChangeCurrency(amount, type, showCounter);
    }

    public static void AddRocksAndPebbles(int amount)
    {
        ChangeCurrency(amount, CurrencyType.Money, true);
    }

    public static void TakeRocksAndPebbles(int amount)
    {
        if (playerData != null)
        {
            playerData.rocksAndPebbles -= amount;
            if (playerData.rocksAndPebbles < 0) playerData.rocksAndPebbles = 0;
            // Optionally update counter here
        }
    }

    public static int GetCurrencyAmount(CurrencyType type)
    {
        if (playerData == null) return 0;
        if (type == CurrencyType.Shard) return playerData.ShellShards;
        return playerData.rocksAndPebbles;
    }

    private static void ChangeCurrency(int amount, CurrencyType type, bool showCounter = true)
    {
        if (playerData == null)
        {
            Debug.LogError("Cannot change currency: PlayerData is null!");
            return;
        }

        ProcessAddCurrency(amount, type, showCounter);
    }

    private static void ProcessAddCurrency(int amount, CurrencyType type, bool showCounter = true)
    {
        // For UI feedback, one might do: CurrencyCounter.RefreshStartCount(type);

        if (type != CurrencyType.Money)
        {
            if (type == CurrencyType.Shard)
            {
                int shellShards = playerData.ShellShards;
                playerData.AddShards(amount);
                amount = playerData.ShellShards - shellShards;
            }
        }
        else
        {
            int oldRocks = playerData.rocksAndPebbles;
            playerData.AddRocksAndPebbles(amount);
            amount = playerData.rocksAndPebbles - oldRocks;
        }

        if (showCounter)
        {
            // For UI feedback: CurrencyCounter.Add(amount, type);
            Debug.Log($"[CurrencyManager] Counter added {amount} of {type}");
        }
    }
}
