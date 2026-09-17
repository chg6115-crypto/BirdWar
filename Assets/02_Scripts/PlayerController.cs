using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private UnitStats stats;

    [SerializeField]
    private float backwardSpeedMultiplier = 0.6f;

    [SerializeField]
    private float rotationSpeed = 120f;

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

        float moveInput = 0f;
        float rotateInput = 0f;

        if (Keyboard.current.wKey.isPressed)
            moveInput += 1f;

        if (Keyboard.current.sKey.isPressed)
            moveInput -= 1f;

        if (Keyboard.current.aKey.isPressed)
            rotateInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            rotateInput += 1f;

        transform.Rotate(
            0f,
            rotateInput * rotationSpeed * Time.deltaTime,
            0f
        );

        float moveSpeed = stats.MoveSpeed;

        if (moveInput < 0f)
            moveSpeed *= backwardSpeedMultiplier;

        transform.position +=
            transform.forward *
            moveInput *
            moveSpeed *
            Time.deltaTime;
    }
}