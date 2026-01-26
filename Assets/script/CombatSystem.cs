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
        // Listen to Server Approval for Damage
        if (FakeServer.Instance != null)
        {
            FakeServer.Instance.OnDamageApproved += ApplyDamageLocally;
        }
    }

    public void RequestDamage(GameObject target, int amount)
    {
        if (GameModeManager.Instance.IsMultiplayer())
        {
            // Send request to server
            FakeServer.Instance.RequestDamage(amount, target.name);
            
            // In a real game, we wait for server response.
            // For now, FakeServer fires event immediately.
            // We pass the "target" implicitly via the event handling logic below.
            // Note: A real implementation would need a NetworkID.
        }
        else
        {
            // Singleplayer: Immediate
            ApplyDamageToTarget(target, amount);
        }
    }

    // Callback from Server
    void ApplyDamageLocally(int amount)
    {
        // In this stub, we don't know who the target was because FakeServer event is simple.
        // For the sake of this refactor, we will assume the Client who asked knows.
        // But properly, we should pass TargetID back and forth.
        Debug.Log($"[CombatSystem] Server approved {amount} damage (Stub Application)");
    }

    public void ApplyDamageToTarget(GameObject target, int amount)
    {
        if (target != null && target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(amount);
        }
    }
}
