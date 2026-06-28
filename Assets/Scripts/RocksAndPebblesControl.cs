using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RocksAndPebblesControl : CurrencyObjectBase
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Add random scatter force
        Vector2 randomForce = new Vector2(Random.Range(-3f, 3f), Random.Range(3f, 7f));
        rb.AddForce(randomForce, ForceMode2D.Impulse);
    }

    protected override bool Collected()
    {
        if (!this.hasValueReference)
        {
            return false;
        }
        CurrencyManager.AddRocksAndPebbles(this.valueReference);
        return true;
    }
}
