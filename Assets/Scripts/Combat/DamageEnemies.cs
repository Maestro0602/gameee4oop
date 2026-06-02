using UnityEngine;

public class DamageEnemies : MonoBehaviour
{
    [Header("Attack Settings")]
    public int baseDamage = 1;
    public float knockbackForce = 15f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HealthManager enemyHealth = collision.GetComponent<HealthManager>();

        if (enemyHealth != null)
        {
            Vector2 direction = collision.transform.position - transform.position;
            float hitAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            HitInstance hit = new HitInstance(
                gameObject, 
                baseDamage, 
                knockbackForce, 
                hitAngle
            );

            enemyHealth.TakeHit(hit);

            if (HeroController.instance != null)
            {
                // In a robust implementation, HeroController could handle bounce or recoil here.
                // HeroController.instance.ReportSuccessfulStrike(collision.transform);
            }
        }
    }
}
