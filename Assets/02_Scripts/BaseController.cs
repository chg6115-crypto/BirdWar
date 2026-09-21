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
    [SerializeField] private UnitTeam unitTeam;

    [Header("State")]
    [SerializeField] private BaseState currentState = BaseState.Active;

    public BaseState CurrentState => currentState;
    public UnitTeam.Team CurrentTeam => unitTeam.CurrentTeam;

    void Awake()
    {
        if (baseHealth == null)
            baseHealth = GetComponent<BaseHealth>();

        if (spawner == null)
            spawner = GetComponent<Spawner>();

        if (unitTeam == null)
            unitTeam = GetComponent<UnitTeam>();

        if (baseHealth == null)
            Debug.LogError($"{gameObject.name}: BaseHealth가 없습니다.");

        if (spawner == null)
            Debug.LogError($"{gameObject.name}: Spawner가 없습니다.");

        if (unitTeam == null)
            Debug.LogError($"{gameObject.name}: UnitTeam이 없습니다.");
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
        if (currentState != BaseState.Active)
            return;

        currentState = BaseState.Destroyed;

        if (spawner != null)
            spawner.StopSpawning();

        Debug.Log($"{gameObject.name}: Base 파괴 상태 진입 / Spawner 정지");
    }

    public void StartCapturing()
    {
        if (currentState == BaseState.Destroyed)
            currentState = BaseState.Capturing;
    }

    public void StopCapturing()
    {
        if (currentState == BaseState.Capturing)
            currentState = BaseState.Destroyed;
    }

    public void Capture(UnitTeam.Team newTeam)
    {
        if (unitTeam == null)
            return;

        unitTeam.SetTeam(newTeam);

        // 점령 완료 후 다시 사용할 수 있는 Base 상태로 변경
        currentState = BaseState.Active;

        Debug.Log($"{gameObject.name}: 점령 완료 / 새 진영 = {newTeam}");
    }
}