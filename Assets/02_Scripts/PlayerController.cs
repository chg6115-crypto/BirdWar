using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private UnitStats stats;

    [Header("Movement")]
    [SerializeField] private float backwardSpeedMultiplier = 0.7f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;

    void Awake()
    {
        stats = GetComponent<UnitStats>();

        if (stats == null)
            Debug.LogError($"{gameObject.name}: UnitStats가 없습니다.");
    }

    void Update()
    {
        if (stats == null || Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        Vector3 moveDirection =
            transform.forward * vertical +
            transform.right * horizontal;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        float moveSpeed = stats.MoveSpeed;

        // S 방향으로 움직일 때만 후진 감속
        if (vertical < 0f)
            moveSpeed *= backwardSpeedMultiplier;

        bool isRunning =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        if (isRunning)
            moveSpeed *= runSpeedMultiplier;

        transform.position +=
            moveDirection *
            moveSpeed *
            Time.deltaTime;
    }
}