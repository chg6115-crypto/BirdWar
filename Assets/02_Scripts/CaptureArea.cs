using System.Collections.Generic;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseController baseController;
    [SerializeField] private BattleArea battleArea;

    [Header("Capture Settings")]
    [SerializeField] private float oneUnitCaptureTime = 10f;
    [SerializeField] private float maxSpeedCaptureTime = 3f;
    [SerializeField] private int maxEffectiveUnits = 5;

    [Header("Debug")]
    [SerializeField, Range(-1f, 1f)]
    private float captureProgress = 0f;

    private readonly Dictionary<UnitTeam, int> unitColliderCounts =
        new Dictionary<UnitTeam, int>();

    void Awake()
    {
        if (baseController == null)
            baseController = GetComponentInParent<BaseController>();

        if (baseController == null)
            Debug.LogError($"{gameObject.name}: BaseController를 찾을 수 없습니다.");

        if (battleArea == null)
            Debug.LogError($"{gameObject.name}: BattleArea가 연결되지 않았습니다.");
    }

    void Update()
    {
        CleanupDestroyedUnits();

        if (baseController == null)
            return;

        // Base가 파괴되거나 점령 중인 상태에서만 점령 가능
        if (baseController.CurrentState != BaseController.BaseState.Destroyed &&
            baseController.CurrentState != BaseController.BaseState.Capturing)
        {
            return;
        }

        int duckCount = GetTeamCount(UnitTeam.Team.Duck);
        int chickenCount = GetTeamCount(UnitTeam.Team.Chicken);

        // 아무도 없음
        if (duckCount == 0 && chickenCount == 0)
        {
            baseController.StopCapturing();
            return;
        }

        // 양쪽이 동시에 존재하면 점령 정지
        if (duckCount > 0 && chickenCount > 0)
        {
            baseController.StartCapturing();
            return;
        }

        baseController.StartCapturing();

        if (duckCount > 0)
        {
            UpdateCaptureProgress(UnitTeam.Team.Duck, duckCount);
        }
        else if (chickenCount > 0)
        {
            UpdateCaptureProgress(UnitTeam.Team.Chicken, chickenCount);
        }
    }

    private void UpdateCaptureProgress(UnitTeam.Team capturingTeam, int unitCount)
    {
        int effectiveUnits = Mathf.Clamp(unitCount, 1, maxEffectiveUnits);

        float t = (effectiveUnits - 1f) /
                  Mathf.Max(1f, maxEffectiveUnits - 1f);

        float captureTime = Mathf.Lerp(
            oneUnitCaptureTime,
            maxSpeedCaptureTime,
            t
        );

        float progressPerSecond = 1f / captureTime;

        if (capturingTeam == UnitTeam.Team.Duck)
            captureProgress += progressPerSecond * Time.deltaTime;
        else
            captureProgress -= progressPerSecond * Time.deltaTime;

        captureProgress = Mathf.Clamp(captureProgress, -1f, 1f);

        if (captureProgress >= 1f)
        {
            CompleteCapture(UnitTeam.Team.Duck);
        }
        else if (captureProgress <= -1f)
        {
            CompleteCapture(UnitTeam.Team.Chicken);
        }
    }

    private void CompleteCapture(UnitTeam.Team newTeam)
    {
        baseController.Capture(newTeam);

        if (battleArea != null)
        {
            BattleArea.AreaOwner newOwner =
                newTeam == UnitTeam.Team.Duck
                    ? BattleArea.AreaOwner.Duck
                    : BattleArea.AreaOwner.Chicken;

            battleArea.SetOwner(newOwner);
        }

        captureProgress = 0f;

        Debug.Log(
            $"{gameObject.name}: 점령 성공 / " +
            $"Base 및 BattleArea → {newTeam}"
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        UnitTeam unitTeam = other.GetComponentInParent<UnitTeam>();

        if (unitTeam == null)
            return;

        // Base 자체의 UnitTeam은 제외
        if (unitTeam.gameObject == baseController.gameObject)
            return;

        if (unitColliderCounts.ContainsKey(unitTeam))
        {
            unitColliderCounts[unitTeam]++;
            return;
        }

        unitColliderCounts.Add(unitTeam, 1);

        Debug.Log(
            $"{gameObject.name}: {unitTeam.CurrentTeam} 진입 / " +
            $"Duck {GetTeamCount(UnitTeam.Team.Duck)} : " +
            $"Chicken {GetTeamCount(UnitTeam.Team.Chicken)}"
        );
    }

    private void OnTriggerExit(Collider other)
    {
        UnitTeam unitTeam = other.GetComponentInParent<UnitTeam>();

        if (unitTeam == null)
            return;

        if (!unitColliderCounts.ContainsKey(unitTeam))
            return;

        unitColliderCounts[unitTeam]--;

        if (unitColliderCounts[unitTeam] <= 0)
        {
            unitColliderCounts.Remove(unitTeam);

            Debug.Log(
                $"{gameObject.name}: {unitTeam.CurrentTeam} 이탈 / " +
                $"Duck {GetTeamCount(UnitTeam.Team.Duck)} : " +
                $"Chicken {GetTeamCount(UnitTeam.Team.Chicken)}"
            );
        }
    }

    private int GetTeamCount(UnitTeam.Team team)
    {
        int count = 0;

        foreach (UnitTeam unit in unitColliderCounts.Keys)
        {
            if (unit != null && unit.CurrentTeam == team)
                count++;
        }

        return count;
    }

    private void CleanupDestroyedUnits()
    {
        List<UnitTeam> destroyedUnits = null;

        foreach (UnitTeam unit in unitColliderCounts.Keys)
        {
            if (unit != null)
                continue;

            destroyedUnits ??= new List<UnitTeam>();
            destroyedUnits.Add(unit);
        }

        if (destroyedUnits == null)
            return;

        foreach (UnitTeam unit in destroyedUnits)
            unitColliderCounts.Remove(unit);
    }
}