using UnityEngine;

public class BaseController : MonoBehaviour
{
    public enum BaseState
    {
        Active,
        Destroyed,
        Capturing
    }

    [Header("References")]
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private Spawner spawner;

    [Header("State")]
    [SerializeField] private BaseState currentState = BaseState.Active;

    public BaseState CurrentState => currentState;

    void Awake()
    {
        if (baseHealth == null)
            baseHealth = GetComponent<BaseHealth>();

        if (spawner == null)
            spawner = GetComponent<Spawner>();

        if (baseHealth == null)
            Debug.LogError($"{gameObject.name}: BaseHealth가 없습니다.");

        if (spawner == null)
            Debug.LogError($"{gameObject.name}: Spawner가 없습니다.");
    }

    void OnEnable()
    {
        if (baseHealth != null)
            baseHealth.Destroyed += HandleBaseDestroyed;
    }

    void OnDisable()
    {
        if (baseHealth != null)
            baseHealth.Destroyed -= HandleBaseDestroyed;
    }

    private void HandleBaseDestroyed(BaseHealth destroyedBase)
    {
        if (currentState == BaseState.Destroyed)
            return;

        currentState = BaseState.Destroyed;

        if (spawner != null)
            spawner.StopSpawning();

        Debug.Log(
            $"{gameObject.name}: Base 파괴 상태 진입 / Spawner 정지"
        );

        // 다음 단계:
        // CaptureArea가 Destroyed 상태를 확인하고 점령을 시작합니다.
    }
}
