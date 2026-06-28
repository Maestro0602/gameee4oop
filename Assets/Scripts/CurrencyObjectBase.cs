using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class CurrencyObjectBase : MonoBehaviour
{
    protected bool hasValueReference = true;
    public int valueReference = 1;

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.GetComponent<HeroController>() != null)
        {
            if (Collected())
            {
                // Play sound or particle effects here
                Destroy(gameObject);
            }
        }
    }

    protected abstract bool Collected();
}
