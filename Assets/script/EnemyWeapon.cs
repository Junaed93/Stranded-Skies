using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
        if (collision.CompareTag("Player"))
        {
            PlayerCombat playerCombat = collision.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(damage);
                Debug.Log("Player hit by enemy weapon");
            }
        }
    }
}
