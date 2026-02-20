using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
    public static MatchInfo instance;
    
    //Left team information
    [Header("Left team information")]
    public List<PlayerInput> leftControllers = new List<PlayerInput>(); // This is assigned in GameControlsConfigPanel.cs
    public Team leftTeam; // This is assigned in Se
    public Formation leftTeamLineUp;
    public string leftTeamUniform;

    //Right team information
    [Header("Right team information")]
    public List<PlayerInput> rightControllers = new List<PlayerInput>(); // This is assigned in GameControlsConfigPanel.cs
    public Team rightTeam;
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
        if (instance == null)
            instance = this;
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