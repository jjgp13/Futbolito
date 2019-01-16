﻿[System.Serializable]
public class MatchTourInfo{

    public TeamTourInfo teamL;
    public int localGoals;
    public TeamTourInfo teamV;
    public int visitGoals;
    public int matchNumber;

    public MatchTourInfo(TeamTourInfo local_team, int local_goals, TeamTourInfo visit_team, int visit_goals, int match_number)
    {
        teamL = local_team;
        localGoals = local_goals;
        teamV = visit_team;
        visitGoals = visit_goals;
        matchNumber = match_number;
    }
}
