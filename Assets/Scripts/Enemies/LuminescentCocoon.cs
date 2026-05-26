using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LuminescentCocoon : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private bool bomb = true;
    [SerializeField] private GameObject explosionPrefab;

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.CompareTag("Nail Attack") || otherCollider.CompareTag("Hero Spell") || otherCollider.CompareTag("HeroBox"))
        {
            Burst();
        }
    }

    private void Burst()
    {
        if (bomb && explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.localRotation);
        }

        // Removed audio portion based on your instructions.
        Destroy(gameObject);
    }
}