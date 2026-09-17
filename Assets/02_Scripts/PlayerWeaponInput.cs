using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponInput : MonoBehaviour
{
    private WeaponController weapon;

    void Awake()
    {
        weapon = GetComponent<WeaponController>();

        if (weapon == null)
            Debug.LogError($"{gameObject.name}: WeaponController가 없습니다.");
    }

    void Update()
    {
        if (weapon == null || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed)
            weapon.TryFire();
    }
}
