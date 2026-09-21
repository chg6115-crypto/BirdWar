using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private UnitTeam.Team team;

    [Header("Spawn")]
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Battlefield")]
    [SerializeField] private BattleFieldManager battleFieldManager;

    [Header("Lane")]
    [SerializeField] private float laneHalfWidth = 3f;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 20f;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnDelay = 0.3f;

    private Coroutine spawnCoroutine;
    private bool isSpawning;
    private int nextFrontIndex;

    private BattleArea homeArea;

    private void Start()
    {
        homeArea = GetComponentInParent<BattleArea>();

        if (battleFieldManager == null)
            battleFieldManager = FindAnyObjectByType<BattleFieldManager>();

        if (homeArea == null)
        {
            Debug.LogError($"{gameObject.name}: 부모 BattleArea를 찾을 수 없습니다.");
            return;
        }

        if (battleFieldManager == null)
        {
            Debug.LogError($"{gameObject.name}: BattleFieldManager를 찾을 수 없습니다.");
            return;
        }

        if (spawnPrefab == null || spawnPoint == null)
        {
            Debug.LogError($"{gameObject.name}: Spawn Prefab 또는 Spawn Point가 없습니다.");
            return;
        }

        StartSpawning();
    }

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log($"{gameObject.name}: Spawner 정지");
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            yield return StartCoroutine(SpawnUnits());

            if (!isSpawning)
                yield break;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SpawnUnits()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (!isSpawning)
                yield break;

            SpawnUnit();

            if (i < spawnCount - 1)
                yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnUnit()
    {
        IReadOnlyList<BattleArea> fronts =
            battleFieldManager.GetAttackableAreas(team);

        if (fronts == null || fronts.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 현재 공격 가능한 Area가 없습니다.");
            return;
        }

        BattleArea targetArea =
            fronts[nextFrontIndex % fronts.Count];

        nextFrontIndex++;

        List<BattleArea> areaPath =
            battleFieldManager.FindPath(homeArea, targetArea);

        if (areaPath.Count < 2)
        {
            Debug.LogWarning(
                $"{gameObject.name}: {homeArea.name} → {targetArea.name} 경로가 없습니다."
            );
            return;
        }

        List<Transform> waypoints = new();

        // 시작 Area는 이미 서 있으므로 제외하고,
        // 다음 Area부터 Destination을 따라갑니다.
        for (int i = 1; i < areaPath.Count; i++)
        {
            if (areaPath[i].Destination != null)
                waypoints.Add(areaPath[i].Destination);
        }

        Transform finalTarget = GetFinalTarget(targetArea);

        if (finalTarget != null &&
            (waypoints.Count == 0 || waypoints[waypoints.Count - 1] != finalTarget))
        {
            waypoints.Add(finalTarget);
        }

        GameObject spawnedUnit = Instantiate(
            spawnPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        UnitTeam spawnedTeam = spawnedUnit.GetComponent<UnitTeam>();

        if (spawnedTeam != null)
            spawnedTeam.SetTeam(team);

        AIController aiController =
            spawnedUnit.GetComponent<AIController>();

        if (aiController == null)
        {
            Debug.LogWarning($"{spawnedUnit.name}: AIController가 없습니다.");
            return;
        }

        float laneOffset = Random.Range(-laneHalfWidth, laneHalfWidth);
        aiController.SetPath(waypoints, laneOffset);

        Debug.Log(
            $"{gameObject.name}: {spawnedUnit.name} → {targetArea.name} / " +
            $"경로 {areaPath.Count} Area / Lane {laneOffset:F1}"
        );
    }

    private Transform GetFinalTarget(BattleArea targetArea)
    {
        BaseController areaBase = targetArea.AreaBase;

        if (areaBase != null &&
            areaBase.CurrentTeam != team &&
            areaBase.CurrentState != BaseController.BaseState.Destroyed)
        {
            return areaBase.transform;
        }

        return targetArea.Destination;
    }
}
