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

    [SerializeField] private float spawnInterval = 20f;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnDelay = 0.3f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(SpawnUnits());
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SpawnUnits()
    {
        for (int i = 0; i < spawnCount; i++)
        {
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
        {
            unitTeam.SetTeam(team);
        }
        else
        {
            Debug.LogWarning(
                $"{spawnedUnit.name}: UnitTeam이 없습니다."
            );
        }

        AIController aiController =
            spawnedUnit.GetComponent<AIController>();

        if (aiController != null)
        {
            aiController.SetDestination(destination);
        }
        else
        {
            Debug.LogWarning(
                $"{spawnedUnit.name}: AIController가 없습니다."
            );
        }
    }
}
