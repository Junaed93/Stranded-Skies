using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        // [Multiplayer] In a real implementation, we would listen to NetworkManager events here.
    }

    public void RequestDamage(GameObject target, int amount)
    {
        if (GameSession.Instance.mode == GameMode.Multiplayer)
        {
            // Multiplayer: Request damage from server
            // NetworkManager.Instance.SendDamageRequest(target.name, amount);
            Debug.Log($"[CombatSystem] Requested {amount} damage on {target.name} (Waiting for Server)");
        }
        else
        {
            // Singleplayer: Immediate
            ApplyDamageToTarget(target, amount);
        }
    }

    // Callback from Server (to be called by NetworkManager)
    public void ApplyDamageLocally(GameObject target, int amount)
    {
        ApplyDamageToTarget(target, amount);
    }

    public void ApplyDamageToTarget(GameObject target, int amount)
    {
        if (target != null && target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(amount);
        }
    }
}
