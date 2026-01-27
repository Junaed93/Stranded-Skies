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
    void Start()
    {
    }
    }

    public void RequestDamage(GameObject target, int amount)
    {
        if (GameSession.Instance.mode == GameMode.Multiplayer)
        {
        if (GameSession.Instance.mode == GameMode.Multiplayer)
        {
            Debug.Log($"[CombatSystem] Requested {amount} damage on {target.name} (Waiting for Server)");
        }
        else
        {
        else
        {
            ApplyDamageToTarget(target, amount);
        }
        }
    }

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
