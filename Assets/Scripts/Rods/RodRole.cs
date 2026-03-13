using UnityEngine;

public enum RodRole
{
    Goalkeeper = 0,
    Defense = 1,
    Midfield = 2,
    Attack = 3,
}

public static class RodRoleExtensions
{
    public static RodRole FromRodName(string rodName)
    {
        return rodName switch
        {
            "GoalKepperRod" => RodRole.Goalkeeper,
            "DefenseRod" => RodRole.Defense,
            "MidfieldRod" => RodRole.Midfield,
            "AttackerRod" => RodRole.Attack,
            _ => RodRole.Midfield,
        };
    }
}
