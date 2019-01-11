[System.Serializable]
public class TeamTourInfo{

    public string name;
    public string group;
    public int w;
    public int d;
    public int l;
    public int gf;
    public int gc;
    public int p;

    public TeamTourInfo(string teamName, string teamGroup, int winnigs, int draws, int losses, int goalsScored, int goalsRecieved, int points)
    {
        name = teamName;
        group = teamGroup;
        w = winnigs;
        d = draws;
        l = losses;
        gf = goalsScored;
        gc = goalsRecieved;
        p = points;
    }
}
