using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private float rotationSpeed = 5f;

    [SerializeField]
    private float stopDistance = 1.5f;

    void Update()
    {
        if (target == null)
            return;

        Vector3 direction =
            target.position - transform.position;

        // 높이 차이는 무시
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stopDistance)
            return;

        // Duck 방향으로 회전
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Enemy가 바라보는 방향으로 이동
        transform.position +=
            transform.forward *
            moveSpeed *
            Time.deltaTime;
    }
}