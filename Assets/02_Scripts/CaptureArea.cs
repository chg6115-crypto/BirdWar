using System.Collections.Generic;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseController baseController;
    [SerializeField] private UnitTeam baseTeam;

    private readonly HashSet<UnitTeam> unitsInside =
        new HashSet<UnitTeam>();

    void Awake()
    {
        if (baseController == null)
            baseController = GetComponentInParent<BaseController>();

        if (baseTeam == null)
            baseTeam = GetComponentInParent<UnitTeam>();

        if (baseController == null)
            Debug.LogError($"{gameObject.name}: BaseController를 찾을 수 없습니다.");

        if (baseTeam == null)
            Debug.LogError($"{gameObject.name}: Base의 UnitTeam을 찾을 수 없습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        UnitTeam unitTeam = other.GetComponentInParent<UnitTeam>();

        if (unitTeam == null)
            return;

        // Base 자신의 Collider 등을 감지하는 경우 제외
        if (unitTeam == baseTeam)
            return;

        // 같은 유닛의 여러 Collider가 들어와도 한 번만 등록
        if (unitsInside.Add(unitTeam))
        {
            Debug.Log(
                $"{gameObject.name}: {unitTeam.CurrentTeam} 진입 / " +
                $"점령 가능 유닛 수: {GetEnemyCount()}"
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        UnitTeam unitTeam = other.GetComponentInParent<UnitTeam>();

        if (unitTeam == null)
            return;

        if (unitsInside.Remove(unitTeam))
        {
            Debug.Log(
                $"{gameObject.name}: {unitTeam.CurrentTeam} 이탈 / " +
                $"점령 가능 유닛 수: {GetEnemyCount()}"
            );
        }
    }

    public int GetEnemyCount()
    {
        if (baseTeam == null)
            return 0;

        unitsInside.RemoveWhere(unit =>
            unit == null
        );

        int count = 0;

        foreach (UnitTeam unit in unitsInside)
        {
            if (unit.CurrentTeam != baseTeam.CurrentTeam)
                count++;
        }

        return count;
    }
}