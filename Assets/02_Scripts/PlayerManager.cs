using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private GameObject startingDuck;

    [Header("Possession")]
    [SerializeField] private float possessionDelay = 3f;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraController cameraController;

    [Header("Selection")]
    [SerializeField] private LayerMask selectionLayerMask = ~0;

    private GameObject currentPlayerUnit;
    private bool canPossess = true;
    private bool selectingUnit = false;

    public GameObject CurrentPlayerUnit => currentPlayerUnit;
    public bool CanPossess => canPossess;

    void Start()
    {
        if (startingDuck != null)
            Possess(startingDuck);
    }

    void Update()
    {
        if (!selectingUnit ||
            !canPossess ||
            mainCamera == null ||
            Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TrySelectDuck();
    }

    public void Possess(GameObject unit)
    {
        if (!canPossess || unit == null)
            return;

        UnitTeam unitTeam = unit.GetComponent<UnitTeam>();
        UnitHealth unitHealth = unit.GetComponent<UnitHealth>();
        AIController aiController = unit.GetComponent<AIController>();
        PlayerController playerController =
            unit.GetComponent<PlayerController>();

        if (unitTeam == null ||
            unitHealth == null ||
            aiController == null ||
            playerController == null)
        {
            Debug.LogWarning(
                $"{unit.name}: 빙의에 필요한 컴포넌트가 없습니다."
            );
            return;
        }

        if (unitHealth.IsDead)
            return;

        if (unitTeam.CurrentTeam != UnitTeam.Team.Duck)
        {
            Debug.LogWarning(
                $"{unit.name}: Duck 팀이 아니므로 빙의할 수 없습니다."
            );
            return;
        }

        if (currentPlayerUnit != null &&
            currentPlayerUnit != unit)
        {
            ReleaseCurrentUnit();
        }

        aiController.enabled = false;
        playerController.enabled = true;

        currentPlayerUnit = unit;

        unitHealth.Died -= HandlePlayerUnitDied;
        unitHealth.Died += HandlePlayerUnitDied;

        selectingUnit = false;

        if (cameraController != null)
        {
            cameraController.SetTarget(unit);
            cameraController.LockCursor();
        }

        Debug.Log($"{unit.name} 빙의 완료");
    }

    public void ReleaseCurrentUnit()
    {
        if (currentPlayerUnit == null)
            return;

        UnitHealth unitHealth =
            currentPlayerUnit.GetComponent<UnitHealth>();

        AIController aiController =
            currentPlayerUnit.GetComponent<AIController>();

        PlayerController playerController =
            currentPlayerUnit.GetComponent<PlayerController>();

        if (unitHealth != null)
            unitHealth.Died -= HandlePlayerUnitDied;

        if (aiController != null)
            aiController.enabled = true;

        if (playerController != null)
            playerController.enabled = false;

        currentPlayerUnit = null;
    }

    private void HandlePlayerUnitDied(UnitHealth deadUnit)
    {
        if (currentPlayerUnit == null)
            return;

        if (deadUnit.gameObject != currentPlayerUnit)
            return;

        deadUnit.Died -= HandlePlayerUnitDied;

        currentPlayerUnit = null;
        canPossess = false;
        selectingUnit = false;

        if (cameraController != null)
        {
            cameraController.SetTarget(null);
            cameraController.UnlockCursor();
        }

        Debug.Log(
            $"플레이어 Duck 사망 - {possessionDelay}초 관전 시작"
        );

        StartCoroutine(PossessionCooldown());
    }

    private IEnumerator PossessionCooldown()
    {
        yield return new WaitForSeconds(possessionDelay);

        canPossess = true;
        selectingUnit = true;

        if (cameraController != null)
            cameraController.UnlockCursor();

        Debug.Log("빙의 가능 - 화면의 아군 Duck을 클릭하세요.");
    }

    private void TrySelectDuck()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            1000f,
            selectionLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        UnitTeam selectedTeam =
            hit.collider.GetComponentInParent<UnitTeam>();

        if (selectedTeam == null)
            return;

        if (selectedTeam.CurrentTeam != UnitTeam.Team.Duck)
            return;

        UnitHealth selectedHealth =
            selectedTeam.GetComponent<UnitHealth>();

        if (selectedHealth == null || selectedHealth.IsDead)
            return;

        Possess(selectedTeam.gameObject);
    }
}
