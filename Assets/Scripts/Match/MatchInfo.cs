using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is attached to a game object with the same name.
/// This will be in charge of set all game objects in GameMatchScene
/// </summary>
public class MatchInfo : MonoBehaviour {

    //Type of match
    public enum MatchType
    {
        QuickMatch,
        TourMatch,
        OnlineMatch
    };

    //Singleton
    public static MatchInfo _matchInfo;
    //Type of match to play
    public MatchType matchType;
    
    //Information of the player
    public Team leftTeam;
    public Formation leftTeamLineUp;
    public string leftTeamUniform;
    //Information of the NPC
    public Team rightTeam;
    public Formation rightTeamLineUp;
    public string rightTeamUniform;

    //Match time and difficulty
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