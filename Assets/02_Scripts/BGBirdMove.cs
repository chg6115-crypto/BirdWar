using UnityEngine;

public class BGBirdMove : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Up / Down Movement")]
    [SerializeField] private float verticalAmount = 0.5f;
    [SerializeField] private float verticalSpeed = 1f;

    [Header("Side Movement")]
    [SerializeField] private float sideAmount = 0.3f;
    [SerializeField] private float sideSpeed = 0.7f;

    private float startY;
    private float time;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        time += Time.deltaTime;

        // 앞으로 천천히 이동
        transform.position +=
            transform.forward * moveSpeed * Time.deltaTime;

        Vector3 position = transform.position;

        // 처음 높이를 기준으로 위아래 이동
        position.y =
            startY + Mathf.Sin(time * verticalSpeed) * verticalAmount;

        // 좌우로 약간 흔들림
        position +=
            transform.right *
            Mathf.Sin(time * sideSpeed) *
            sideAmount *
            Time.deltaTime;

        transform.position = position;
    }
}