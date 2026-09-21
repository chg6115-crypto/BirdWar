using System.Collections.Generic;
using UnityEngine;

public class BattleFieldManager : MonoBehaviour
{
    [Header("Areas - Auto")]
    [SerializeField] private List<BattleArea> allAreas = new();

    [Header("Debug - Attackable Areas")]
    [SerializeField] private List<BattleArea> duckAttackableAreas = new();
    [SerializeField] private List<BattleArea> chickenAttackableAreas = new();

    public IReadOnlyList<BattleArea> AllAreas => allAreas;
    public IReadOnlyList<BattleArea> DuckAttackableAreas => duckAttackableAreas;
    public IReadOnlyList<BattleArea> ChickenAttackableAreas => chickenAttackableAreas;

    void Start()
    {
        FindAllAreas();
        InitializeAreaConnections();
        RefreshFronts();
    }

    private void FindAllAreas()
    {
        allAreas.Clear();
        allAreas.AddRange(GetComponentsInChildren<BattleArea>());
        Debug.Log($"BattleFieldManager: BattleArea {allAreas.Count}개 등록");
    }

    private void InitializeAreaConnections()
    {
        foreach (BattleArea area in allAreas)
        {
            if (area != null)
                area.FindConnectedAreas();
        }
    }

    public void RefreshFronts()
    {
        duckAttackableAreas.Clear();
        chickenAttackableAreas.Clear();

        foreach (BattleArea area in allAreas)
        {
            if (area == null)
                continue;

            if (area.CanBeAttackedBy(BattleArea.AreaOwner.Duck))
                duckAttackableAreas.Add(area);

            if (area.CanBeAttackedBy(BattleArea.AreaOwner.Chicken))
                chickenAttackableAreas.Add(area);
        }

        Debug.Log($"Duck 공격 가능 Area: {GetAreaNames(duckAttackableAreas)}");
        Debug.Log($"Chicken 공격 가능 Area: {GetAreaNames(chickenAttackableAreas)}");
    }

    public IReadOnlyList<BattleArea> GetAttackableAreas(UnitTeam.Team team)
    {
        return team == UnitTeam.Team.Duck
            ? duckAttackableAreas
            : chickenAttackableAreas;
    }

    public List<BattleArea> FindPath(BattleArea start, BattleArea goal)
    {
        List<BattleArea> emptyPath = new();

        if (start == null || goal == null)
            return emptyPath;

        Queue<BattleArea> queue = new();
        Dictionary<BattleArea, BattleArea> cameFrom = new();

        queue.Enqueue(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            BattleArea current = queue.Dequeue();

            if (current == goal)
                break;

            foreach (BattleArea next in current.ConnectedAreas)
            {
                if (next == null || cameFrom.ContainsKey(next))
                    continue;

                queue.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return emptyPath;

        List<BattleArea> path = new();
        BattleArea step = goal;

        while (step != null)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }

    private string GetAreaNames(List<BattleArea> areas)
    {
        if (areas.Count == 0)
            return "없음";

        List<string> names = new();

        foreach (BattleArea area in areas)
        {
            if (area != null)
                names.Add(area.name);
        }

        return string.Join(", ", names);
    }
}
