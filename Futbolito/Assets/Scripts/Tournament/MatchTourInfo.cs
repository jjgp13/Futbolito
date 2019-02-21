/// <summary>
/// This class is serialized and saved inside the tournament controller info.
/// </summary>

[System.Serializable]
public class MatchTourInfo{

    public TeamTourInfo localTeam;
    public int localGoals;
    public TeamTourInfo visitTeam;
    public int visitGoals;
    public int matchNumber;

    public MatchTourInfo(TeamTourInfo local_team, int local_goals, TeamTourInfo visit_team, int visit_goals, int match_number)
    {
        localTeam = local_team;
        localGoals = local_goals;
        visitTeam = visit_team;
        visitGoals = visit_goals;
        matchNumber = match_number;
    }

    public MatchTourInfo(MatchTourInfo info)
    {
        localTeam = info.localTeam;
        localGoals = info.localGoals;
        visitTeam = info.visitTeam;
        visitGoals = info.visitGoals;
        matchNumber = info.matchNumber;
    }
}
