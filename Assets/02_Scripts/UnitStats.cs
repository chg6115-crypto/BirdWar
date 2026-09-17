using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int maxHealth = 100;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField]
    private float detectionRange = 10f;

    [SerializeField]
    private float attackRange = 8f;

    [SerializeField]
    private int attackDamage = 10;

    [SerializeField]
    private float attackCooldown = 1f;

    public int MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;
    public float AttackCooldown => attackCooldown;
}