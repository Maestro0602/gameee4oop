using UnityEngine;

using UnityEngine;

public class PlayerCombatResource2D : MonoBehaviour
{
    [SerializeField] private float maxResource = 100f;
    [SerializeField] private float startingResource = 0f;

    public float CurrentResource { get; private set; }
    public float MaxResource => maxResource;
    public bool IsFull => CurrentResource >= maxResource;
    public float Normalized => maxResource <= 0f ? 0f : CurrentResource / maxResource;

    private void Awake()
    {
        CurrentResource = Mathf.Clamp(startingResource, 0f, maxResource);
    }

    public void Gain(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        CurrentResource = Mathf.Clamp(CurrentResource + amount, 0f, maxResource);
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (CurrentResource < amount)
        {
            return false;
        }

        CurrentResource -= amount;
        return true;
    }

    public void SpendAll()
    {
        CurrentResource = 0f;
    }
}
