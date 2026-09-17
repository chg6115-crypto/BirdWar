using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 500;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => currentHealth <= 0;

    public event Action<BaseHealth> Destroyed;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            $"{gameObject.name} Base 데미지: {damage} / " +
            $"남은 HP: {currentHealth}"
        );

        if (currentHealth <= 0)
            DestroyBase();
    }

    private void DestroyBase()
    {
        Debug.Log($"{gameObject.name} Base 파괴!");

        Destroyed?.Invoke(this);

        // GameObject는 제거하지 않습니다.
        // 이후 점령 시스템에서 같은 Base를 계속 사용합니다.
    }
}
