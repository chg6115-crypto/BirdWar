using System.Collections.Generic;
using UnityEngine;

public class TerritoryCapturePoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleArea battleArea;
    [SerializeField] private GameObject duckFlag;
    [SerializeField] private GameObject chickenFlag;

    [Header("Capture Settings")]
    [SerializeField] private float oneUnitCaptureTime = 10f;
    [SerializeField] private float maxSpeedCaptureTime = 3f;
    [SerializeField] private int maxEffectiveUnits = 5;

    [Header("Debug")]
    [SerializeField, Range(-1f, 1f)]
    private float captureProgress = 0f;

    private readonly Dictionary<UnitTeam, int> unitColliderCounts = new();

    void Awake()
    {
        if (battleArea == null)
            battleArea = GetComponentInParent<BattleArea>();

        if (battleArea == null)
        {
            Debug.LogError($"{gameObject.name}: BattleArea를 찾을 수 없습니다.");
            return;
        }

        UpdateFlagVisual();
    }

    void Update()
    {
        if (battleArea == null)
            return;

        CleanupDestroyedUnits();

        int duckCount = GetTeamCount(UnitTeam.Team.Duck);
        int chickenCount = GetTeamCount(UnitTeam.Team.Chicken);

        // 아무도 없거나 양 진영이 함께 있으면 진행을 멈추고
        // 현재 점령 진행도는 그대로 유지합니다.
        if ((duckCount == 0 && chickenCount == 0) ||
            (duckCount > 0 && chickenCount > 0))
        {
            return;
        }

        if (duckCount > 0)
        {
            if (battleArea.Owner == BattleArea.AreaOwner.Duck)
                return;

            UpdateCaptureProgress(UnitTeam.Team.Duck, duckCount);
        }
        else
        {
            if (battleArea.Owner == BattleArea.AreaOwner.Chicken)
                return;

            UpdateCaptureProgress(UnitTeam.Team.Chicken, chickenCount);
        }
    }

    private void UpdateCaptureProgress(UnitTeam.Team team, int unitCount)
    {
        int effectiveUnits = Mathf.Clamp(
            unitCount,
            1,
            maxEffectiveUnits
        );

        float t =
            (effectiveUnits - 1f) /
            Mathf.Max(1f, maxEffectiveUnits - 1f);

        float captureTime = Mathf.Lerp(
            oneUnitCaptureTime,
            maxSpeedCaptureTime,
            t
        );

        float progressPerSecond = 1f / captureTime;

        if (team == UnitTeam.Team.Duck)
            captureProgress += progressPerSecond * Time.deltaTime;
        else
            captureProgress -= progressPerSecond * Time.deltaTime;

        captureProgress = Mathf.Clamp(
            captureProgress,
            -1f,
            1f
        );

        if (captureProgress >= 1f)
            CompleteCapture(BattleArea.AreaOwner.Duck);
        else if (captureProgress <= -1f)
            CompleteCapture(BattleArea.AreaOwner.Chicken);
    }

    private void CompleteCapture(BattleArea.AreaOwner newOwner)
    {
        battleArea.SetOwner(newOwner);
        captureProgress = 0f;

        UpdateFlagVisual();

        Debug.Log(
            $"{gameObject.name}: 일반 지역 점령 완료 → " +
            $"{battleArea.name} = {newOwner}"
        );
    }

    private void UpdateFlagVisual()
    {
        if (duckFlag != null)
        {
            duckFlag.SetActive(
                battleArea.Owner == BattleArea.AreaOwner.Duck
            );
        }

        if (chickenFlag != null)
        {
            chickenFlag.SetActive(
                battleArea.Owner == BattleArea.AreaOwner.Chicken
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        UnitTeam unit =
            other.GetComponentInParent<UnitTeam>();

        if (unit == null)
            return;

        // Base 등은 제외하고 실제 전투 유닛만 셉니다.
        UnitHealth health =
            unit.GetComponent<UnitHealth>();

        if (health == null)
            return;

        if (unitColliderCounts.ContainsKey(unit))
        {
            unitColliderCounts[unit]++;
            return;
        }

        unitColliderCounts.Add(unit, 1);

        Debug.Log(
            $"{gameObject.name}: {unit.CurrentTeam} 진입 / " +
            $"Duck {GetTeamCount(UnitTeam.Team.Duck)} : " +
            $"Chicken {GetTeamCount(UnitTeam.Team.Chicken)}"
        );
    }

    private void OnTriggerExit(Collider other)
    {
        UnitTeam unit =
            other.GetComponentInParent<UnitTeam>();

        if (unit == null)
            return;

        if (!unitColliderCounts.ContainsKey(unit))
            return;

        unitColliderCounts[unit]--;

        if (unitColliderCounts[unit] <= 0)
            unitColliderCounts.Remove(unit);
    }

    private int GetTeamCount(UnitTeam.Team team)
    {
        int count = 0;

        foreach (UnitTeam unit in unitColliderCounts.Keys)
        {
            if (unit != null &&
                unit.CurrentTeam == team)
            {
                count++;
            }
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
