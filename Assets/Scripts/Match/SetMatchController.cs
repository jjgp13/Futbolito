using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This scprit handles the match setup, given the information in the match info object passed in the matchMenuSettings
/// Set table
/// Set grass pattern
/// Set left team and right team
/// </summary>
[DefaultExecutionOrder(0)]
public class SetMatchController : MonoBehaviour
{
    public bool isTestingMatchScene = false;

    [Header("Reference to table selected")]
    public SpriteRenderer grassPattern;
    public SpriteRenderer tableColor;

    [Header("Reference to left/right team gameobjects")]
    public GameObject leftTeamGameObject;
    public GameObject rightTeamGameObject;

    public MatchType matchType;
    public Sprite ballSelected;
    public Sprite grassSelected;
    public Sprite tableSelected;
    public int matchTime;
    public int matchLevel;

    //This is for testing
    //When matchInfo is not initialized
    [Space(20)]
    [Header("-----------Team Selected (Testing)----------")]
    [SerializeField] private GameObject playersController;
    public Team leftTeamInformation;
    public Team rightTeamInformation;
    public GameObject NoControlsPanel;

    // Start is called before the first frame update
    void Awake()
    {
        if(MatchInfo.instance != null) 
        {
            SetMatchFromMatchInfoGameObject();
            this.enabled = false;
        }
        else
        {
            SetMatchSceneTestingGameplay();
        }
    }

    //private void Update()
    //{
    //    if (FindAnyObjectByType<PlayersInputController>().playerInputs.Count > 0)
    //    {
    //        SetMatchGameObjectWhenPlayerJoined();
    //    }
    //}

    private void SetMatchFromMatchInfoGameObject()
    {
        //Set table and grass selected
        grassPattern.sprite = MatchInfo.instance.grassSelected;
        tableColor.sprite = MatchInfo.instance.tableSelected;

        //Set team side scripts
        SetInputsForTeam(leftTeamGameObject, MatchInfo.instance.leftControllers.Count);
        SetInputsForTeam(rightTeamGameObject, MatchInfo.instance.leftControllers.Count);


        //Set UI (flags and team names)
        SetTeamFlags("LeftTeamFlags", MatchInfo.instance.leftTeam.flag, MatchInfo.instance.leftTeam.teamName);
        SetTeamFlags("RightTeamFlags", MatchInfo.instance.rightTeam.flag, MatchInfo.instance.rightTeam.teamName);
    }

    /// <summary>
    /// Given the amount of player on each team.
    /// Active or deactive component that will handle the movement of the team lines
    /// </summary>
    /// <param name="team">Left or right team game object</param>
    /// <param name="teamPlayers">Number of players assigned to that team</param>
    private void SetInputsForTeam(GameObject team, int teamPlayersCount)
    {
        //If there are at least one player.
        //Lines will be handle by controller input
        if (teamPlayersCount > 0)
        {
            team.GetComponent<TeamRodsController>().enabled = true;
            team.GetComponent<AITeamRodsController>().enabled = false;
        }
        //If not, lines will be handle by AI
        else
        {
            team.GetComponent<TeamRodsController>().enabled = false;
            team.GetComponent<AITeamRodsController>().enabled = true;
        }
    }

    /// <summary>
    /// This method will search for game objects with the tag given and it will set the flag and the name of the team
    /// </summary>
    /// <param name="tag">Objects to search by tag</param>
    /// <param name="flag">Team flag</param>
    /// <param name="name">Team name</param>
    public void SetTeamFlags(string tag, Sprite flag, string name)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < flags.Length; i++)
        {
            //Get the image and set the flag
            flags[i].GetComponent<Image>().sprite = flag;
            //Get its first child and set its name
            flags[i].transform.GetChild(0).GetComponent<Text>().text = name;
        }
    }

    /*
     These methods are for testing the scene
     */
    private void SetMatchSceneTestingGameplay()
    {
        //Testing scene, it means there are no players in the scene
        leftTeamGameObject.SetActive(false);
        rightTeamGameObject.SetActive(false);

        //Set UI (flags and team names)
        SetTeamFlags("LeftTeamFlags", leftTeamInformation.flag, leftTeamInformation.teamName);
        SetTeamFlags("RightTeamFlags", rightTeamInformation.flag, rightTeamInformation.teamName);

        Instantiate(playersController);
        NoControlsPanel.SetActive(true);
    }
}
