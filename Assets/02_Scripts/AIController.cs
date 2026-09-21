using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform destination;
    [SerializeField] private float waypointReachDistance = 1.5f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationStrength = 1.5f;

    private readonly List<Transform> path = new();
    private int pathIndex;
    private float laneOffset;

    private UnitTeam unitTeam;
    private UnitStats stats;
    private WeaponController weapon;

    private Transform currentTarget;
    private UnitHealth currentTargetHealth;

    void Awake()
    {
        unitTeam = GetComponent<UnitTeam>();
        stats = GetComponent<UnitStats>();
        weapon = GetComponent<WeaponController>();

        if (unitTeam == null)
            Debug.LogError($"{gameObject.name}: UnitTeam이 없습니다.");

        if (stats == null)
            Debug.LogError($"{gameObject.name}: UnitStats가 없습니다.");

        if (weapon == null)
            Debug.LogError($"{gameObject.name}: WeaponController가 없습니다.");
    }

    void Update()
    {
        if (unitTeam == null || stats == null || weapon == null)
            return;

        FindNearestEnemy();

        if (currentTarget != null)
        {
            HandleEnemyUnit();
            return;
        }

        HandlePath();
    }

    public void SetDestination(Transform newDestination)
    {
        path.Clear();
        pathIndex = 0;
        laneOffset = 0f;
        destination = newDestination;
    }

    public void SetPath(List<Transform> newPath, float newLaneOffset)
    {
        path.Clear();

        if (newPath != null)
            path.AddRange(newPath);

        pathIndex = 0;
        laneOffset = newLaneOffset;
        destination = path.Count > 0 ? path[path.Count - 1] : null;
    }

    private void HandlePath()
    {
        if (path.Count == 0)
        {
            HandleDestination();
            return;
        }

        if (pathIndex >= path.Count)
            return;

        Transform waypoint = path[pathIndex];

        if (waypoint == null)
        {
            pathIndex++;
            return;
        }

        bool isFinalWaypoint = pathIndex == path.Count - 1;

        if (isFinalWaypoint)
        {
            destination = waypoint;

            BaseHealth baseHealth =
                destination.GetComponentInParent<BaseHealth>();

            UnitTeam baseTeam =
                destination.GetComponentInParent<UnitTeam>();

            bool isEnemyBase =
                baseHealth != null &&
                baseTeam != null &&
                !unitTeam.IsSameTeam(baseTeam);

            if (isEnemyBase && !baseHealth.IsDestroyed)
            {
                HandleEnemyBase(baseHealth);
                return;
            }
        }

        Vector3 targetPosition = GetLaneTarget(pathIndex);
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= waypointReachDistance)
        {
            pathIndex++;
            return;
        }

        MoveInDirection(
            direction.normalized + GetSeparationDirection()
        );
    }

    private Vector3 GetLaneTarget(int index)
    {
        Vector3 center = path[index].position;

        Vector3 segmentDirection;

        if (index == 0)
        {
            segmentDirection = center - transform.position;
        }
        else
        {
            segmentDirection =
                center - path[index - 1].position;
        }

        segmentDirection.y = 0f;

        if (segmentDirection.sqrMagnitude < 0.001f)
            return center;

        Vector3 side =
            Vector3.Cross(Vector3.up, segmentDirection.normalized);

        return center + side * laneOffset;
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

            if (otherTeam == null ||
                otherTeam == unitTeam ||
                unitTeam.IsSameTeam(otherTeam))
            {
                continue;
            }

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

    private void HandleEnemyUnit()
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

        if (distance > stats.AttackRange)
        {
            MoveInDirection(
                direction.normalized + GetSeparationDirection()
            );
            return;
        }

        FaceDirection(direction);

        Vector3 targetPosition =
            currentTarget.position + Vector3.up * 0.5f;

        if (weapon.TryAimBallistic(targetPosition))
        {
            weapon.TryFire();
            return;
        }

        MoveInDirection(
            direction.normalized + GetSeparationDirection()
        );
    }

    private void HandleDestination()
    {
        if (destination == null)
            return;

        BaseHealth baseHealth =
            destination.GetComponentInParent<BaseHealth>();

        UnitTeam baseTeam =
            destination.GetComponentInParent<UnitTeam>();

        bool isEnemyBase =
            baseHealth != null &&
            baseTeam != null &&
            !unitTeam.IsSameTeam(baseTeam);

        if (isEnemyBase && !baseHealth.IsDestroyed)
        {
            HandleEnemyBase(baseHealth);
            return;
        }

        Vector3 direction =
            destination.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.25f)
            return;

        MoveInDirection(
            direction.normalized + GetSeparationDirection()
        );
    }

    private void HandleEnemyBase(BaseHealth baseHealth)
    {
        Vector3 baseDirection =
            destination.position - transform.position;

        baseDirection.y = 0f;

        float baseDistance = baseDirection.magnitude;

        if (baseDistance > stats.AttackRange)
        {
            MoveInDirection(
                baseDirection.normalized + GetSeparationDirection()
            );
            return;
        }

        Collider baseCollider =
            baseHealth.GetComponentInChildren<Collider>();

        Vector3 targetPosition =
            baseCollider != null
                ? baseCollider.bounds.center
                : baseHealth.transform.position + Vector3.up * 0.5f;

        FaceDirection(baseDirection);

        if (weapon.TryAimBallistic(targetPosition))
        {
            weapon.TryFire();
            return;
        }

        MoveInDirection(
            baseDirection.normalized + GetSeparationDirection()
        );
    }

    private void MoveInDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        FaceDirection(direction);

        transform.position +=
            transform.forward *
            stats.MoveSpeed *
            Time.deltaTime;
    }

    private Vector3 GetSeparationDirection()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            separationRadius
        );

        Vector3 separation = Vector3.zero;
        int nearbyUnitCount = 0;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            UnitTeam nearbyTeam =
                nearbyCollider.GetComponentInParent<UnitTeam>();

            if (nearbyTeam == null ||
                nearbyTeam.gameObject == gameObject ||
                !unitTeam.IsSameTeam(nearbyTeam))
            {
                continue;
            }

            UnitHealth nearbyHealth =
                nearbyTeam.GetComponent<UnitHealth>();

            if (nearbyHealth == null)
                continue;

            Vector3 awayDirection =
                transform.position - nearbyTeam.transform.position;

            awayDirection.y = 0f;

            float distance = awayDirection.magnitude;

            if (distance < 0.001f)
                continue;

            float weight =
                1f - Mathf.Clamp01(distance / separationRadius);

            separation += awayDirection.normalized * weight;
            nearbyUnitCount++;
        }

        if (nearbyUnitCount == 0)
            return Vector3.zero;

        separation /= nearbyUnitCount;

        return separation * separationStrength;
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
}
