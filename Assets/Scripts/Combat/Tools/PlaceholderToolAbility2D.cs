using UnityEngine;

using UnityEngine;

public class PlaceholderToolAbility2D : MonoBehaviour, ICombatToolAbility
{
    [SerializeField] private int resourceCost = 20;
    [SerializeField] private float upwardBoost = 3f;
    [SerializeField] private float forwardBoost = 2f;

    public int ResourceCost => resourceCost;

    public bool TryUse(PlayerController2D controller)
    {
        if (controller == null)
        {
            return false;
        }

        Rigidbody2D rb = controller.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            return false;
        }

        float facing = controller.transform.localScale.x >= 0f ? 1f : -1f;
        rb.linearVelocity += new Vector2(forwardBoost * facing, upwardBoost);

        Debug.Log("Placeholder tool used: applied a small mobility boost.");
        return true;
    }
}
