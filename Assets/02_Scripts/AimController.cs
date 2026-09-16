using UnityEngine;

public class AimController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private float maxHeadAngle = 135f;
    [SerializeField] private float maxUpAngle = 45f;
    [SerializeField] private float maxDownAngle = 25f;

    [SerializeField] private float aimDistance = 100f;

    [SerializeField] private LayerMask aimLayerMask;

    private Quaternion startLocalRotation;

    void Start()
    {
        startLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Ray cameraRay = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 aimPoint =
            cameraRay.origin +
            cameraRay.direction * aimDistance;

        if (Physics.Raycast(
            cameraRay,
            out RaycastHit hit,
            aimDistance,
            aimLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        Vector3 worldDirection =
            aimPoint - transform.position;

        Vector3 localDirection =
            transform.parent.InverseTransformDirection(
                worldDirection.normalized
            );

        float yaw = Mathf.Atan2(
            localDirection.x,
            localDirection.z
        ) * Mathf.Rad2Deg;

        float horizontalDistance = new Vector2(
            localDirection.x,
            localDirection.z
        ).magnitude;

        float pitch = -Mathf.Atan2(
            localDirection.y,
            horizontalDistance
        ) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(
            yaw,
            -maxHeadAngle,
            maxHeadAngle
        );

        pitch = Mathf.Clamp(
            pitch,
            -maxUpAngle,
            maxDownAngle
        );

        Quaternion targetRotation =
            startLocalRotation *
            Quaternion.Euler(pitch, yaw, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}