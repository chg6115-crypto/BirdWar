using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private float fireCooldown = 0.2f;

    private float lastFireTime = -999f;

    void Update()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            Fire();
        }
    }

    void Fire()
    {
        if (Time.time < lastFireTime + fireCooldown)
            return;

        Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        lastFireTime = Time.time;
    }
}