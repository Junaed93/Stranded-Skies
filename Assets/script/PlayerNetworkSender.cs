using UnityEngine;

public static class PlayerNetworkSender
{
    public static void SendAttack()
    {
        Debug.Log("[PlayerNetworkSender] SendAttack - Stub called");
        // Stub for later implementation
    }

    public static void SendMove(Vector2 velocity)
    {
        Debug.Log($"[PlayerNetworkSender] SendMove({velocity}) - Stub called");
        // Stub for later implementation
    }

    public static void RequestDamage(GameObject target, int amount)
    {
        Debug.Log($"[PlayerNetworkSender] RequestDamage to {target.name} for {amount} - Stub called");
        // Stub for later implementation
    }
}
