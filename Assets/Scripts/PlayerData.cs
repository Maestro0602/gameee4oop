using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    [Header("Currency")]
    public int rocksAndPebbles = 0;
    public int ShellShards = 0;

    [Header("Health")]
    public int health = 5;

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

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddRocksAndPebbles(int amount)
    {
        rocksAndPebbles += amount;
        Debug.Log($"Added {amount} Rocks and Pebbles. Total: {rocksAndPebbles}");
    }

    public void AddShards(int amount)
    {
        ShellShards += amount;
        Debug.Log($"Added {amount} Shards. Total Shards: {ShellShards}");
    }
}
