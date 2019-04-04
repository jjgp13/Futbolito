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
    public Team leftTeam;
    public int leftControllers;
    public Formation leftTeamLineUp;
    public string leftTeamUniform;
    
    //Right team information
    public Team rightTeam;
    public int rightControllers;
    public Formation rightTeamLineUp;
    public string rightTeamUniform;

    //Match settings information
    //Type of match to play
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