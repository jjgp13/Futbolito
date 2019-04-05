using UnityEngine;
using UnityEngine.SceneManagement;

//Type of match
public enum MatchType
{
    QuickMatch,
    TourMatch,
    OnlineMatch
};

/// <summary>
/// This class is attached to a game object with the same name.
/// This will be in charge of set all game objects in GameMatchScene
/// </summary>
public class MatchInfo : MonoBehaviour {

    //Singleton
    public static MatchInfo _matchInfo;
    
    //Left team information
    [Header("Left team information")]
    public Team leftTeam;
    public int defenderLeftController;
    public int attackerLeftController;
    public Formation leftTeamLineUp;
    public string leftTeamUniform;

    //Right team information
    [Header("Right team information")]
    public Team rightTeam;
    public int defenderRightController;
    public int attackerRightController;
    public Formation rightTeamLineUp;
    public string rightTeamUniform;

    //Match settings information
    //Type of match to play
    [Header("Match settings information")]
    public MatchType matchType;
    public Sprite ballSelected;
    public Sprite grassSelected;
    public Sprite tableSelected;
    public int matchTime;
    public int matchLevel;

    private void Awake()
    {
        _matchInfo = this;
        DontDestroyOnLoad(this);
    }

    //When this object is starting, it will be assign a MatchType given the scene in which is created.
    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "QuickMatchMenu") matchType = MatchType.QuickMatch;
        else if (scene.name == "TourMainMenu") matchType = MatchType.TourMatch;
    }
}