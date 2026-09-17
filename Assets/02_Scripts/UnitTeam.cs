using UnityEngine;

public class UnitTeam : MonoBehaviour
{
    public enum Team
    {
        Duck,
        Chicken
    }

    [SerializeField]
    private Team team;

    public Team CurrentTeam => team;

    public void SetTeam(Team newTeam)
    {
        team = newTeam;
    }

    public bool IsSameTeam(UnitTeam other)
    {
        if (other == null)
            return false;

        return team == other.team;
    }
}
