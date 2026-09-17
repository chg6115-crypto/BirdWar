using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform destination;

    private UnitTeam unitTeam;
    private UnitStats stats;
    private Transform currentTarget;
    private UnitHealth currentTargetHealth;

    private float lastAttackTime = -999f;

    void Awake()
    {
        unitTeam = GetComponent<UnitTeam>();
        stats = GetComponent<UnitStats>();

        if (unitTeam == null)
            Debug.LogError($"{gameObject.name}: UnitTeam이 없습니다.");

        if (stats == null)
            Debug.LogError($"{gameObject.name}: UnitStats가 없습니다.");
    }

    void Update()
    {
        if (unitTeam == null || stats == null)
            return;

        FindNearestEnemy();

        if (currentTarget != null)
            HandleEnemy();
        else
            MoveToDestination();
    }

    public void SetDestination(Transform newDestination)
    {
        destination = newDestination;
    }

    private void FindNearestEnemy()
    {
        Collider[] detectedColliders = Physics.OverlapSphere(
            transform.position,
            stats.DetectionRange
        );

        Transform nearestEnemy = null;
        UnitHealth nearestEnemyHealth = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider detectedCollider in detectedColliders)
        {
            UnitTeam otherTeam =
                detectedCollider.GetComponentInParent<UnitTeam>();

            if (otherTeam == null)
                continue;

            if (otherTeam == unitTeam)
                continue;

            if (unitTeam.IsSameTeam(otherTeam))
                continue;

            UnitHealth otherHealth =
                otherTeam.GetComponent<UnitHealth>();

            if (otherHealth == null || otherHealth.IsDead)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                otherTeam.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = otherTeam.transform;
                nearestEnemyHealth = otherHealth;
            }
        }

        currentTarget = nearestEnemy;
        currentTargetHealth = nearestEnemyHealth;
    }

    private void HandleEnemy()
    {
        if (currentTargetHealth == null || currentTargetHealth.IsDead)
        {
            currentTarget = null;
            currentTargetHealth = null;
            return;
        }

        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        FaceDirection(direction);

        if (distance > stats.AttackRange)
        {
            MoveForward();
            return;
        }

        Attack();
    }

    private void Attack()
    {
        if (currentTargetHealth == null)
            return;

        if (Time.time < lastAttackTime + stats.AttackCooldown)
            return;

        currentTargetHealth.TakeDamage(stats.AttackDamage);
        lastAttackTime = Time.time;
    }

    private void MoveToDestination()
    {
        if (destination == null)
            return;

        Vector3 direction =
            destination.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.1f)
            return;

        FaceDirection(direction);
        MoveForward();
    }

    private void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            5f * Time.deltaTime
        );
    }

    private void MoveForward()
    {
        transform.position +=
            transform.forward *
            stats.MoveSpeed *
            Time.deltaTime;
    }
}
