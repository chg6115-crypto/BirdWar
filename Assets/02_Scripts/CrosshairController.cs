using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private RectTransform headCrosshair;

    [SerializeField]
    private float maxAimDistance = 100f;

    [SerializeField]
    private LayerMask aimLayerMask;

    void LateUpdate()
    {
        if (mainCamera == null ||
            firePoint == null ||
            headCrosshair == null)
            return;

        Vector3 aimPoint;

        Ray ray = new Ray(
            firePoint.position,
            firePoint.forward
        );

        // FirePoint 방향으로 Raycast
        // Bullet Layer와 Trigger는 조준 대상으로 사용하지 않음
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxAimDistance,
            aimLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint =
                firePoint.position +
                firePoint.forward * maxAimDistance;
        }

        // 실제 조준 위치를 화면 좌표로 변환
        Vector3 screenPoint =
            mainCamera.WorldToScreenPoint(aimPoint);

        if (screenPoint.z > 0f)
        {
            headCrosshair.gameObject.SetActive(true);
            headCrosshair.position = screenPoint;
        }
        else
        {
            headCrosshair.gameObject.SetActive(false);
        }
    }
}