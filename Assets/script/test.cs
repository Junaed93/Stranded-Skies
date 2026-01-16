using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private Animator[] EnemyAnims;

    // =========================
    // IDLE
    // =========================
    public void Animation_Idle()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            Debug.Log(anim.gameObject.name + " is Idling");
        }
    }

    // =========================
    // RUN
    // =========================
    public void Animation_Run()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", true);
            Debug.Log(anim.gameObject.name + " is Running");
        }
    }

    // =========================
    // HIT
    // =========================
    public void Animation_Hit()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            anim.SetTrigger("Hit");
            Debug.Log(anim.gameObject.name + " is Hit");
        }
    }

    // =========================
    // DEATH
    // =========================
    public void Animation_Death()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            anim.SetTrigger("Death");
            Debug.Log(anim.gameObject.name + " Died");
        }
    }

    // =========================
    // ABILITY (TRIGGER)
    // =========================
    public void Animation_Ability()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            anim.SetTrigger("Ability");
            Debug.Log(anim.gameObject.name + " used Ability");
        }
    }

    // =========================
    // ATTACKS
    // =========================
    public void Animation_Attack1()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            anim.SetTrigger("Attack");
            Debug.Log(anim.gameObject.name + " Attack 1");
        }
    }

    public void Animation_Attack2()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            anim.SetTrigger("Attack2");
            Debug.Log(anim.gameObject.name + " Attack 2");
        }
    }

    public void Animation_Attack3()
    {
        foreach (Animator anim in EnemyAnims)
        {
            if (!anim.gameObject.activeSelf) continue;

            anim.SetBool("Run", false);
            anim.SetTrigger("Attack3");
            Debug.Log(anim.gameObject.name + " Attack 3");
        }
    }
}
