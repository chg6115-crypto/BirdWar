using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.2f;

    private UnitTeam unitTeam;
    private UnitStats stats;
    private float lastFireTime = -999f;

    public Transform FirePoint => firePoint;

    void Awake()
    {
        unitTeam = GetComponent<UnitTeam>();
        stats = GetComponent<UnitStats>();

        if (unitTeam == null)
            Debug.LogError($"{gameObject.name}: UnitTeam이 없습니다.");

        if (stats == null)
            Debug.LogError($"{gameObject.name}: UnitStats가 없습니다.");
    }

    public bool TryAimBallistic(Vector3 targetPosition)
    {
        if (bulletPrefab == null || firePoint == null)
            return false;

        Bullet bullet = bulletPrefab.GetComponent<Bullet>();

        if (bullet == null)
            return false;

        Rigidbody bulletRb =
            bulletPrefab.GetComponent<Rigidbody>();

        if (bulletRb == null || bulletRb.mass <= 0f)
            return false;

        float speed = Mathf.Sqrt(
            bullet.LaunchForce * bullet.LaunchForce +
            bullet.UpwardForce * bullet.UpwardForce
        ) / bulletRb.mass;

        if (speed <= 0f)
            return false;

        Vector3 toTarget =
            targetPosition - firePoint.position;

        Vector3 flatDirection =
            new Vector3(toTarget.x, 0f, toTarget.z);

        float horizontalDistance = flatDirection.magnitude;

        if (horizontalDistance < 0.01f)
            return false;

        float heightDifference = toTarget.y;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float speedSquared = speed * speed;

        float discriminant =
            speedSquared * speedSquared -
            gravity *
            (
                gravity *
                horizontalDistance *
                horizontalDistance +
                2f *
                heightDifference *
                speedSquared
            );

        if (discriminant < 0f)
            return false;

        // 낮은 탄도를 선택합니다.
        float launchAngle = Mathf.Atan(
            (
                speedSquared -
                Mathf.Sqrt(discriminant)
            ) /
            (
                gravity *
                horizontalDistance
            )
        ) * Mathf.Rad2Deg;

        float offsetAngle = Mathf.Atan2(
            bullet.UpwardForce,
            bullet.LaunchForce
        ) * Mathf.Rad2Deg;

        // Bullet 자체의 Upward Force만큼 이미 위로 뜨므로
        // FirePoint는 그 각도만큼 덜 위를 봅니다.
        float firePointAngle =
            launchAngle - offsetAngle;

        Quaternion yawRotation =
            Quaternion.LookRotation(
                flatDirection.normalized,
                Vector3.up
            );

        firePoint.rotation =
            yawRotation *
            Quaternion.Euler(
                -firePointAngle,
                0f,
                0f
            );

        return true;
    }

    public bool TryFire()
    {
        if (!enabled)
            return false;

        if (Time.time < lastFireTime + fireCooldown)
            return false;

        if (bulletPrefab == null ||
            firePoint == null ||
            unitTeam == null ||
            stats == null)
        {
            return false;
        }

        GameObject bulletObject = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Bullet bullet =
            bulletObject.GetComponent<Bullet>();

        if (bullet == null)
        {
            Debug.LogError(
                $"{bulletObject.name}: Bullet 컴포넌트가 없습니다."
            );

            Destroy(bulletObject);
            return false;
        }

        bullet.Initialize(
            unitTeam.CurrentTeam,
            stats.AttackDamage
        );

        lastFireTime = Time.time;
        return true;
    }
}
