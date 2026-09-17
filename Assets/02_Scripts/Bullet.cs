using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float launchForce = 9f;
    [SerializeField] private float upwardForce = 2.5f;
    [SerializeField] private float spreadAngle = 3f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody rb;
    private UnitTeam.Team ownerTeam;
    private int damage;
    private bool initialized = false;

    public float LaunchForce => launchForce;
    public float UpwardForce => upwardForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError($"{gameObject.name}: Rigidbody가 없습니다.");
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public float GetLaunchSpeed()
    {
        if (rb == null)
            return 0f;

        float totalImpulse = Mathf.Sqrt(
            launchForce * launchForce +
            upwardForce * upwardForce
        );

        return totalImpulse / rb.mass;
    }

    public float GetLaunchOffsetAngle()
    {
        return Mathf.Atan2(
            upwardForce,
            launchForce
        ) * Mathf.Rad2Deg;
    }

    public void Initialize(
        UnitTeam.Team team,
        int attackDamage)
    {
        ownerTeam = team;
        damage = attackDamage;
        initialized = true;

        if (rb == null)
            return;

        float randomYaw = Random.Range(
            -spreadAngle,
            spreadAngle
        );

        float randomPitch = Random.Range(
            -spreadAngle * 0.5f,
            spreadAngle * 0.5f
        );

        Quaternion spreadRotation =
            Quaternion.Euler(
                randomPitch,
                randomYaw,
                0f
            );

        Vector3 forward =
            spreadRotation * transform.forward;

        Vector3 up =
            spreadRotation * transform.up;

        Vector3 launchImpulse =
            forward * launchForce +
            up * upwardForce;

        rb.AddForce(
            launchImpulse,
            ForceMode.Impulse
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized)
            return;

        if (other.gameObject.layer ==
            LayerMask.NameToLayer("Environment"))
        {
            Destroy(gameObject);
            return;
        }

        UnitTeam hitTeam =
            other.GetComponentInParent<UnitTeam>();

        if (hitTeam == null)
            return;

        // 아군은 무시
        if (hitTeam.CurrentTeam == ownerTeam)
            return;

        // 적 유닛 피격
        UnitHealth hitHealth =
            hitTeam.GetComponent<UnitHealth>();

        if (hitHealth != null && !hitHealth.IsDead)
        {
            hitHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 적 Base 피격
        BaseHealth baseHealth =
            hitTeam.GetComponent<BaseHealth>();

        if (baseHealth != null && !baseHealth.IsDestroyed)
        {
            baseHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
