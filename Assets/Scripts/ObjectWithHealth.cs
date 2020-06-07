using UnityEngine;

public class ObjectWithHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;

    protected void Awake()
    {
        currentHealth = maxHealth;
    }

    public void DamageBy(int damage)
    {
        ChangeHealth(-damage);
    }

    public void HealBy(int heal)
    {
        ChangeHealth(heal);
    }

    private void ChangeHealth(int healthChange)
    {
        currentHealth = Mathf.Clamp(currentHealth + healthChange, 0, maxHealth);
        if (currentHealth == 0)
        {
            TriggerDeath();
        }
    }

    protected virtual void TriggerDeath()
    {
        Destroy(gameObject);
    }
}
