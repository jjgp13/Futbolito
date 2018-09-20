using UnityEngine;

public class MatchInfo : MonoBehaviour {

    public static MatchInfo _matchInfo;

    public Team playerTeam;
    public Formation playerLineUp;
    public string playerUniform;

    public Team comTeam;
    public Formation comLineUp;
    public string comUniform;

    public int matchTime;
    public int difficulty;

    private void Awake()
    {
        _matchInfo = this;
    }

}