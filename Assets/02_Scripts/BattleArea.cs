using System.Collections.Generic;
using UnityEngine;

public class BattleArea : MonoBehaviour
{
    public enum AreaOwner
    {
        Neutral,
        Duck,
        Chicken
    }

    [Header("Area")]
    [SerializeField] private Vector2Int coordinate;
    [SerializeField] private AreaOwner owner = AreaOwner.Neutral;

    [Header("World")]
    [SerializeField] private Transform destination;

    [Header("Base")]
    [SerializeField] private BaseController areaBase;

    [Header("Connected Areas - Auto")]
    [SerializeField] private List<BattleArea> connectedAreas = new();

    private BattleFieldManager battleFieldManager;

    public Vector2Int Coordinate => coordinate;
    public AreaOwner Owner => owner;
    public Transform Destination => destination;
    public BaseController AreaBase => areaBase;
    public IReadOnlyList<BattleArea> ConnectedAreas => connectedAreas;

    void Awake()
    {
        FindDestination();

        battleFieldManager = GetComponentInParent<BattleFieldManager>();

        if (battleFieldManager == null)
        {
            Debug.LogError(
                $"{gameObject.name}: BattleFieldManager를 찾을 수 없습니다."
            );
        }
    }

    private void FindDestination()
    {
        if (destination != null)
            return;

        Transform foundDestination = transform.Find("Destination");

        if (foundDestination == null)
        {
            Debug.LogError(
                $"{gameObject.name}: Destination을 찾을 수 없습니다."
            );
            return;
        }

        destination = foundDestination;
    }

    public void FindConnectedAreas()
    {
        connectedAreas.Clear();

        if (transform.parent == null)
            return;

        BattleArea[] allAreas =
            transform.parent.GetComponentsInChildren<BattleArea>();

        foreach (BattleArea otherArea in allAreas)
        {
            if (otherArea == this)
                continue;

            Vector2Int difference =
                otherArea.Coordinate - coordinate;

            int distance =
                Mathf.Abs(difference.x) +
                Mathf.Abs(difference.y);

            if (distance == 1)
                connectedAreas.Add(otherArea);
        }

        Debug.Log(
            $"{gameObject.name}: 인접 Area {connectedAreas.Count}개 자동 연결"
        );
    }

    public void SetOwner(AreaOwner newOwner)
    {
        if (owner == newOwner)
            return;

        AreaOwner previousOwner = owner;
        owner = newOwner;

        Debug.Log(
            $"{gameObject.name}: 지역 소유권 변경 " +
            $"{previousOwner} → {owner}"
        );

        if (battleFieldManager != null)
            battleFieldManager.RefreshFronts();
    }

    public bool IsConnectedTo(BattleArea otherArea)
    {
        if (otherArea == null)
            return false;

        return connectedAreas.Contains(otherArea);
    }

    public bool CanBeAttackedBy(AreaOwner attackingTeam)
    {
        if (attackingTeam == AreaOwner.Neutral)
            return false;

        if (owner == attackingTeam)
            return false;

        foreach (BattleArea connectedArea in connectedAreas)
        {
            if (connectedArea == null)
                continue;

            if (connectedArea.Owner == attackingTeam)
                return true;
        }

        return false;
    }
}