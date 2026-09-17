using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    private UnitStats stats;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<UnitHealth> Died;

    void Awake()
    {
        stats = GetComponent<UnitStats>();

        if (stats == null)
        {
            Debug.LogError($"{gameObject.name}: UnitStats가 없습니다.");
            return;
        }

        currentHealth = stats.MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log(
            $"{gameObject.name} 데미지: {damage} / 남은 HP: {currentHealth}"
        );

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망");

        Died?.Invoke(this);

        Destroy(gameObject);
    }
}
