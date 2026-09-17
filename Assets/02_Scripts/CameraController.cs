using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform duck;

    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float lookHeight = 2f;
    [SerializeField] private float sensitivity = 0.08f;
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    [SerializeField, Range(0f, 1f)]
    private float bodyFollowAmount = 0.7f;

    private float yaw;
    private float pitch = 15f;
    private float previousDuckYaw;

    void Start()
    {
        LockCursor();

        if (duck != null)
        {
            previousDuckYaw = duck.eulerAngles.y;
            yaw = previousDuckYaw;
        }
    }

    void LateUpdate()
    {
        if (target == null || duck == null)
            return;

        if (Mouse.current != null &&
            Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * sensitivity;
            pitch -= mouseDelta.y * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        float currentDuckYaw = duck.eulerAngles.y;

        float duckYawDelta = Mathf.DeltaAngle(
            previousDuckYaw,
            currentDuckYaw
        );

        yaw += duckYawDelta * bodyFollowAmount;
        previousDuckYaw = currentDuckYaw;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset =
            rotation * new Vector3(0f, height, -distance);

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookPosition =
            target.position + Vector3.up * lookHeight;

        transform.LookAt(lookPosition);
    }

    public void SetTarget(GameObject newDuck)
    {
        if (newDuck == null)
        {
            target = null;
            duck = null;
            return;
        }

        duck = newDuck.transform;

        Transform newCameraTarget =
            newDuck.transform.Find("CameraTarget");

        if (newCameraTarget == null)
        {
            Debug.LogWarning(
                $"{newDuck.name}: CameraTarget을 찾을 수 없습니다."
            );
            return;
        }

        target = newCameraTarget;

        previousDuckYaw = duck.eulerAngles.y;
        yaw = previousDuckYaw;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
