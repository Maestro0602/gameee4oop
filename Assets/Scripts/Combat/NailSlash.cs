using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class NailSlash : MonoBehaviour
{
    private PolygonCollider2D hitbox;

    private void Awake()
    {
        hitbox = GetComponent<PolygonCollider2D>();
        hitbox.isTrigger = true; 
    }

    public void EnableHitbox()
    {
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }
}
