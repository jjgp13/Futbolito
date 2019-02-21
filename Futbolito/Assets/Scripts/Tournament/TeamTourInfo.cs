/// <summary>
/// This class is serialized and save inside tournament information.
/// </summary>

[System.Serializable]
public class TeamTourInfo{

    public string teamName;
    public string teamGroup;
    public int victories;
    public int knockoutVictories;
    public int draws;
    public int defeats;
    public int knockoutDefeats;
    public int goalsScored;
    public int goalsReceived;
    public int points;

    public TeamTourInfo(string _teamName, string _teamGroup, int _victories, int _knockoutVictories, int _draws, int _defeats, int _knockoutDefeats, int _goalsScored, int _goalsRecieved, int _points)
    {
        teamName = _teamName;
        teamGroup = _teamGroup;
        victories = _victories;
        knockoutVictories = _knockoutVictories;
        draws = _draws;
        defeats = _defeats;
        knockoutDefeats = _knockoutDefeats;
        goalsScored = _goalsScored;
        goalsReceived = _goalsRecieved;
        points = _points;
    }

    public TeamTourInfo(TeamTourInfo info)
    {
        teamName = info.teamName;
        teamGroup = info.teamGroup;
        victories = info.victories;
        knockoutVictories = info.knockoutVictories;
        draws = info.draws;
        defeats = info.defeats;
        knockoutDefeats = info.knockoutDefeats;
        goalsScored = info.goalsScored;
        goalsReceived = info.goalsReceived;
        points = info.points;
    }
}
