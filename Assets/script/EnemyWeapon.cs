using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Player hit by enemy weapon");
            }
        }
    }
}
