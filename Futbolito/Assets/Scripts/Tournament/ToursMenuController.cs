using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is used in TournamentSelectionScene and it controls actions on this scene related to UI.
/// </summary>
public class ToursMenuController : MonoBehaviour {

    //Get data 
    public TournamentController tcInfo;

    //Reference to the scriptable objects that has the information of each tournament
    public Tournament[] tours;
    //Reference to the panel that contains the teams that participate in each tournament
    public GameObject teamsPanel;

    //This prefab contains the flag and the name of the team
    public Button teamButton;

    //Array that handles the tournament buttons
    public Button[] toursBtns;
    //Sprite that turn on a the color of the torunament selected.
    public Sprite selectedTourSprite, notSelectedTourSprite;

    //Grid that contains the teams in teamsPanel
    private GridLayoutGroup teamsLayout;

    //Reference to the map that lays on the background of the scene and the sprites of each confederation.
    public Image tourMapSprite;
    public Sprite[] tourMaps;

    //Reference to team that has been selected.
    public Image teamSelectedFlag;

    //Reference panel not team selected.
    public GameObject notTeamSelectedPanel;
    

    // Use this for initialization
    void Start () {
        teamsLayout = teamsPanel.GetComponent<GridLayoutGroup>();
	}

    /// <summary>
    /// This method is called by the tournament buttons that are on the scene
    /// This will display the teams that participate on the tournament selected.
    /// </summary>
    /// <param name="tourIndex">Indexof the tour selected. tours array</param>
    public void DisplayTeamsOnPanel(int tourIndex)
    {
        //Delete the team that was previously selected if so.
        TournamentController._tourCtlr.teamSelected = "";
        //Delete the teams that are on already present on the panel
        DeleteTeamsFromPanel();
        SetTeamsPanel(tours[tourIndex].teams.Length);
        tourMapSprite.sprite = tourMaps[tourIndex];

        //Get the info of the tournament selected.
        Tournament tour = tours[tourIndex];
        //Iterate the teams present on this tournament and instantiate as button.
        for (int i = 0; i < tour.teams.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            Team team = tour.teams[i];
            newTeam.image.sprite = team.flag;
            newTeam.GetComponent<TeamSelected>().team = team;
            newTeam.transform.GetChild(0).GetComponent<Text>().text = team.teamName;
            newTeam.transform.SetParent(teamsPanel.transform);
        }
    }

    /// <summary>
    /// This method delete the buttons (teams) of the panel that contains them.
    /// </summary>
    void DeleteTeamsFromPanel()
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// This method highlight the torunament selected.
    /// </summary>
    /// <param name="index">Index of the tournament (tours array).</param>
    public void ChangeButtonSprite(int index) {
        for (int i = 0; i < toursBtns.Length; i++)
        {
            if (index == i) toursBtns[index].image.sprite = selectedTourSprite;
            else toursBtns[i].image.sprite = notSelectedTourSprite;
        }
    }

    /// <summary>
    /// This method change the properties of the layout given the number of participants in the tour
    /// </summary>
    /// <param name="teamsN">Number of teams on the tournament</param>
    void SetTeamsPanel(int teamsN)
    {
        if(teamsN == 16)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(128, 104);
            teamsLayout.spacing = new Vector2(55, 15);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else if (teamsN == 12)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(128, 104);
            teamsLayout.spacing = new Vector2(130, 15);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else if (teamsN == 24)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(104, 80);
            teamsLayout.spacing = new Vector2(11, 45);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else
        {
            teamsLayout.padding = new RectOffset(5,5,20,20);
            teamsLayout.cellSize = new Vector2(80, 56);
            teamsLayout.spacing = new Vector2(10, 70);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
    }

    //Change scene
    public void MainMenu(string sceneName)
    {
        Destroy(GameObject.Find("TourController"));
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Save the info of this tournament and change to Tournament main menu scene.
    /// Called by start button in tournament scene.
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    public void StartTournament(string sceneName)
    {
        if (tcInfo.teamSelected != "")
        {
            tcInfo.SaveTour();
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            notTeamSelectedPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Deactive game object that is passing 
    /// </summary>
    /// <param name="obj">Game object that is going to be deactivate</param>
    public void DeactivePanel(GameObject obj)
    {
        obj.SetActive(false);
    }
}
