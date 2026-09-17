using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] private UnitTeam.Team team;

    [Header("Spawn")]
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform destination;

    [Header("Road")]
    [SerializeField] private BoxCollider moveArea;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 20f;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnDelay = 0.3f;

    private Coroutine spawnCoroutine;
    private bool isSpawning;

    private void Start()
    {
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
        if (spawnPrefab == null ||
            spawnPoint == null ||
            destination == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Spawn 설정이 빠져 있습니다."
            );
            return;
        }

        GameObject spawnedUnit = Instantiate(
            spawnPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        UnitTeam unitTeam =
            spawnedUnit.GetComponent<UnitTeam>();

        if (unitTeam != null)
            unitTeam.SetTeam(team);
        else
            Debug.LogWarning(
                $"{spawnedUnit.name}: UnitTeam이 없습니다."
            );

        AIController aiController =
            spawnedUnit.GetComponent<AIController>();

        if (aiController == null)
        {
            Debug.LogWarning(
                $"{spawnedUnit.name}: AIController가 없습니다."
            );
            return;
        }

        aiController.SetDestination(destination);
        aiController.SetMoveArea(moveArea);
    }
}
