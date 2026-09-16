using UnityEngine;
using UnityEngine.InputSystem;

public class DuckController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float backwardSpeed = 3f;

    [SerializeField]
    private float rotationSpeed = 120f;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        float moveInput = 0f;
        float rotateInput = 0f;

        // 전진 / 후진
        if (Keyboard.current.wKey.isPressed)
            moveInput += 1f;

        if (Keyboard.current.sKey.isPressed)
            moveInput -= 1f;

        // 좌회전 / 우회전
        if (Keyboard.current.aKey.isPressed)
            rotateInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            rotateInput += 1f;

        // 회전
        transform.Rotate(
            0f,
            rotateInput * rotationSpeed * Time.deltaTime,
            0f
        );

        // 전진과 후진 속도를 따로 적용
        float currentSpeed =
            moveInput >= 0f ? moveSpeed : backwardSpeed;

        transform.position +=
            transform.forward *
            moveInput *
            currentSpeed *
            Time.deltaTime;
    }
}