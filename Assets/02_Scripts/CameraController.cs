using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform duck;

    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 1.5f;

    // 카메라가 CameraTarget보다 얼마나 위를 바라볼지
    [SerializeField] private float lookHeight = 2f;

    [SerializeField] private float sensitivity = 0.08f;
    [SerializeField] private float followSpeed = 15f;

    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    [SerializeField]
    [Range(0f, 1f)]
    private float bodyFollowAmount = 0.7f;

    private float yaw;
    private float pitch = 15f;

    private float previousDuckYaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (duck != null)
        {
            previousDuckYaw = duck.eulerAngles.y;
            yaw = previousDuckYaw;
        }
    }

    void LateUpdate()
    {
        if (target == null || duck == null || Mouse.current == null)
            return;

        // 마우스로 카메라 회전
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * sensitivity;
        pitch -= mouseDelta.y * sensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Duck이 A/D로 회전하면 카메라도 일정 비율 따라감
        float currentDuckYaw = duck.eulerAngles.y;

        float duckYawDelta = Mathf.DeltaAngle(
            previousDuckYaw,
            currentDuckYaw
        );

        yaw += duckYawDelta * bodyFollowAmount;

        previousDuckYaw = currentDuckYaw;

        // 카메라 위치 계산
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset =
            rotation * new Vector3(0f, height, -distance);

        Vector3 targetPosition =
            target.position + offset;

        // Duck을 부드럽게 추적
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // CameraTarget보다 위쪽을 바라봄
        // → Duck이 화면 아래쪽에 위치하게 됨
        Vector3 lookPosition =
            target.position + Vector3.up * lookHeight;

        transform.LookAt(lookPosition);
    }
}