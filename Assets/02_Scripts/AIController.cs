using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform destination;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationStrength = 1.5f;

    private BoxCollider moveArea;
    private float laneX;

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

        HandleDestination();
    }

    public void SetDestination(Transform newDestination)
    {
        destination = newDestination;
    }

    public void SetMoveArea(BoxCollider newMoveArea)
    {
        moveArea = newMoveArea;

        if (moveArea == null)
            return;

        Bounds bounds = moveArea.bounds;

        laneX = Random.Range(
            bounds.min.x,
            bounds.max.x
        );
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

            // Base는 여기서 제외합니다.
            // Base는 Destination으로 따로 공격합니다.
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
        if (currentTargetHealth == null ||
            currentTargetHealth.IsDead)
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
            Vector3 moveDirection =
                direction.normalized + GetSeparationDirection();

            FaceDirection(moveDirection);
            MoveForward();
            KeepInsideRoad();
            return;
        }

        Vector3 targetPosition =
            currentTarget.position + Vector3.up * 0.5f;

        if (weapon.TryAimBallistic(targetPosition))
        {
            weapon.TryFire();
            return;
        }

        // AttackRange 안에 들어왔어도 현재 탄속으로 닿지 않으면
        // 사거리 계산이 가능한 거리까지 더 접근합니다.
        Vector3 fallbackMoveDirection =
            direction.normalized + GetSeparationDirection();

        FaceDirection(fallbackMoveDirection);
        MoveForward();
        KeepInsideRoad();
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

        // Destination이 Base가 아니거나 이미 파괴된 경우에는
        // 기존처럼 목적지까지 이동합니다.
        MoveToDestination();
    }

    private void HandleEnemyBase(BaseHealth baseHealth)
    {
        // 이동할 때는 Base 중심으로 몰리지 않고
        // Spawn 시 배정받은 각자의 laneX를 유지합니다.
        Vector3 moveTargetPosition = destination.position;

        if (moveArea != null)
        {
            Bounds bounds = moveArea.bounds;

            moveTargetPosition.x = Mathf.Clamp(
                laneX,
                bounds.min.x,
                bounds.max.x
            );
        }

        Vector3 moveDirection =
            moveTargetPosition - transform.position;

        moveDirection.y = 0f;

        // 공격 사거리 판정은 lane상의 이동 목표가 아니라
        // 실제 Base 위치까지의 거리로 계산합니다.
        Vector3 baseDirection =
            destination.position - transform.position;

        baseDirection.y = 0f;

        float baseDistance = baseDirection.magnitude;

        if (baseDistance > stats.AttackRange)
        {
            Vector3 separatedMoveDirection =
                moveDirection.normalized +
                GetSeparationDirection();

            FaceDirection(separatedMoveDirection);
            MoveForward();
            KeepInsideRoad();
            return;
        }

        Collider baseCollider =
            baseHealth.GetComponentInChildren<Collider>();

        Vector3 targetPosition;

        if (baseCollider != null)
        {
            targetPosition = baseCollider.bounds.center;
        }
        else
        {
            targetPosition =
                baseHealth.transform.position +
                Vector3.up * 0.5f;
        }

        // 사격할 때만 Base 중심을 바라봅니다.
        FaceDirection(baseDirection);

        if (weapon.TryAimBallistic(targetPosition))
        {
            weapon.TryFire();
            return;
        }

        // AttackRange 안이지만 현재 탄속으로 닿지 않으면
        // 각자의 lane을 유지한 채 더 접근합니다.
        Vector3 fallbackMoveDirection =
            moveDirection.normalized +
            GetSeparationDirection();

        FaceDirection(fallbackMoveDirection);
        MoveForward();
        KeepInsideRoad();
    }

    private void MoveToDestination()
    {
        if (destination == null)
            return;

        Vector3 targetPosition = destination.position;

        if (moveArea != null)
        {
            Bounds bounds = moveArea.bounds;

            targetPosition.x = Mathf.Clamp(
                laneX,
                bounds.min.x,
                bounds.max.x
            );
        }

        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.1f)
            return;

        Vector3 moveDirection =
            direction.normalized + GetSeparationDirection();

        FaceDirection(moveDirection);
        MoveForward();
        KeepInsideRoad();
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

            if (nearbyTeam == null)
                continue;

            if (nearbyTeam.gameObject == gameObject)
                continue;

            if (!unitTeam.IsSameTeam(nearbyTeam))
                continue;

            // 같은 팀 Base는 Separation 대상에서 제외합니다.
            UnitHealth nearbyHealth =
                nearbyTeam.GetComponent<UnitHealth>();

            if (nearbyHealth == null)
                continue;

            Vector3 awayDirection =
                transform.position -
                nearbyTeam.transform.position;

            awayDirection.y = 0f;

            float distance = awayDirection.magnitude;

            if (distance < 0.001f)
                continue;

            float weight =
                1f - Mathf.Clamp01(
                    distance / separationRadius
                );

            separation +=
                awayDirection.normalized * weight;

            nearbyUnitCount++;
        }

        if (nearbyUnitCount == 0)
            return Vector3.zero;

        separation /= nearbyUnitCount;

        return separation * separationStrength;
    }

    private void KeepInsideRoad()
    {
        if (moveArea == null)
            return;

        Bounds bounds = moveArea.bounds;

        Vector3 position = transform.position;

        position.x = Mathf.Clamp(
            position.x,
            bounds.min.x,
            bounds.max.x
        );

        transform.position = position;
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
