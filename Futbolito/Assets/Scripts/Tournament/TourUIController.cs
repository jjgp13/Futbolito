using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// This class handle all the stuff related to UI in TourMainMenu scene.
/// </summary>
public class TourUIController : MonoBehaviour {

    //Info of the tournament
    private TournamentController tourInfo;
    //Info of the next match
    private MatchInfo _matchInfo;

    [Header("Main menu Panel")]
    [Tooltip("Reference to the UI prefab that shows the information of a team in the tournament. ")]
    public GameObject teamTourStatsPrefab;

    //Reference to the panel that show the teams by group.
    public GameObject positionsPanel;
    //Reference to the text of the group. 
    public Text groupText;

    //Auxiliar to change teams in positions panel
    private int teamsInTour;
    private int indexGroupOfteamSelected;

    //Reference to flags of the next match.
    public Image localTeamFlag;
    public Image visitTeamFlag;

    //Information of the teams for the next match
    //0 is local, 1 is visit
    private Team[] teamsForMatch = new Team[2];

    //Reference to prefab for MatchInfo gameobject.
    public GameObject matchInfo;

    //Reference to animator
    public Animator canvasAnimator;

    [Header("Next match panel settings")]
    public Text roundText;

    public Image playerFlag;
    public Image playerFirstUniform;
    public Image playerSecondUniform;

    public Image npcFlag;
    public Image npcUniform;
    public Text npcFormation;

    public Text matchTimeText;
    public Text matchLevelText;


    //Singleton
    private void Awake()
    {
        tourInfo = TournamentController._tourCtlr;
        _matchInfo = MatchInfo._matchInfo;
    }

    // Use this for initialization
    void Start () {
        //Local team info
        teamsForMatch[0] = GetTeamInformation(tourInfo.playerMatches[tourInfo.matchesRound].localTeam.teamName);
        //Visit team info
        teamsForMatch[1] = GetTeamInformation(tourInfo.playerMatches[tourInfo.matchesRound].visitTeam.teamName);
        //Set flags
        SetFlagsForNextMatch();

        //Get teams amount and index group of team selected.
        teamsInTour = tourInfo.teamsAmount;
        indexGroupOfteamSelected = GetTeamGroupSelectedIndex();
        
        //Set teams in panel.
        //The list was previously order by groups when the tournament was created.
        SetTeamsPositionPanel(OrderTeamsByPoints(tourInfo.teamList));
        ShowTeamsInPanel(indexGroupOfteamSelected);


        groupText.text = "Group " + tourInfo.teamList[indexGroupOfteamSelected].teamGroup;
        roundText.text = "Round " + tourInfo.matchesRound.ToString();
    }

    /// <summary>
    /// Get the team's list ordeder by group and instatiate prefab with team's info
    /// </summary>
    /// <param name="teamList"></param>
    public void SetTeamsPositionPanel(List<TeamTourInfo> teamList)
    {
        for (int i = 0; i < teamsInTour; i++)
        {
            GameObject teamPosition = teamTourStatsPrefab;
            Team teamInfo = Resources.Load<Team>("Teams/" + teamList[i].teamName + "/" + teamList[i].teamName);
            teamPosition.transform.GetChild(0).GetComponent<Image>().sprite = teamInfo.flag;
            teamPosition.transform.GetChild(1).GetComponent<Text>().text = teamList[i].teamName;
            Instantiate(teamPosition).transform.SetParent(positionsPanel.transform);
            positionsPanel.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Order team list by point.
    /// </summary>
    /// <param name="teamList">List of the teams in the tournament</param>
    /// <returns>A list orderer by points.</returns>
    public List<TeamTourInfo> OrderTeamsByPoints(List<TeamTourInfo> teamList)
    {
        List<TeamTourInfo> teamsOrdered = new List<TeamTourInfo>();
        for (int i = 0; i < teamList.Count; i+=4)
        {
            List<TeamTourInfo> groupU = new List<TeamTourInfo>();
            for (int j = i; j < i + 4; j++) groupU.Add(teamList[j]);
            List<TeamTourInfo> groupO = groupU.OrderBy(team => team.points).ToList();
            for (int k = 0; k < groupO.Count; k++) teamsOrdered.Add(groupO[k]);   
        }
        return teamsOrdered;
    }

    /// <summary>
    /// Change the group showed in positions panel.
    /// It is called by the arrows next to the panel.
    /// </summary>
    /// <param name="direction">To which direction want to go in position panel. (Left or right)</param>
    public void ChangeGroupInPositionPanel(string direction)
    {
        if (direction == "left")
        {
            if (indexGroupOfteamSelected == 0)
            {
                HideTeamsInPanel(indexGroupOfteamSelected);
                indexGroupOfteamSelected = teamsInTour - 4;
                ShowTeamsInPanel(indexGroupOfteamSelected);
            }
            else
            {
                HideTeamsInPanel(indexGroupOfteamSelected);
                indexGroupOfteamSelected -= 4;
                ShowTeamsInPanel(indexGroupOfteamSelected);
            }
        }
        if (direction == "right")
        {
            if (indexGroupOfteamSelected == teamsInTour - 4)
            {
                HideTeamsInPanel(indexGroupOfteamSelected);
                indexGroupOfteamSelected = 0;
                ShowTeamsInPanel(indexGroupOfteamSelected);
            }
            else
            {
                HideTeamsInPanel(indexGroupOfteamSelected);
                indexGroupOfteamSelected += 4;
                ShowTeamsInPanel(indexGroupOfteamSelected);
            }
        }
        //Change the letter of the group in the title of the panel. 
        groupText.text = "Group " + tourInfo.teamList[indexGroupOfteamSelected].teamGroup;
    }

    /// <summary>
    /// Activate teams in panel to show them. As teams are in a list, this method uses index to activate index team and four next to it.
    /// </summary>
    /// <param name="index">Index of the first team to activate</param>
    private void ShowTeamsInPanel(int index)
    {
        for (int i = index; i < index+4; i++)
        {
            GameObject team = positionsPanel.transform.GetChild(i).gameObject;
            team.SetActive(true);

            //if the name of the team is equal to the team selected, paint of yellow.
            if(tourInfo.teamSelected == tourInfo.teamList[i].teamName) PaintTeamSelected(team);
        }
    }

    /// <summary>
    /// Hide teams in positions panel.
    /// </summary>
    /// <param name="index">First team to hide and next 4</param>
    private void HideTeamsInPanel(int index)
    {
        for (int i = index; i < index+4; i++)
        {
            GameObject team = positionsPanel.transform.GetChild(i).gameObject;
            team.SetActive(false);
        }
    }

    /// <summary>
    /// Get index of the group in which team selected is.
    /// </summary>
    /// <returns>Index of the group</returns>
    private int GetTeamGroupSelectedIndex()
    {
        int index = 0;
        for (int i = 0; i < tourInfo.teamsAmount; i += 4)
        {
            for (int j = i; j < i + 4; j++)
                if (tourInfo.teamList[j].teamName == tourInfo.teamSelected)
                {
                    //Index of the group team you selected.
                    index = i;
                    //Break first loop
                    i = tourInfo.teamsAmount;
                    //Break this loop
                    break;
                }
        }
        return index;
    }

    /// <summary>
    /// Get the object that has the team selected and paint it of yellow to differentiate.
    /// </summary>
    /// <param name="team"></param>
    private void PaintTeamSelected(GameObject team)
    {
        Text teamName = team.transform.GetChild(1).GetComponent<Text>();
        teamName.color = Color.yellow;
    }

    /// <summary>
    /// Return scriptable object with team's information
    /// </summary>
    /// <param name="teamName">Name of the team</param>
    /// <returns>Scriptable object</returns>
    private Team GetTeamInformation(string teamName)
    {
        return Resources.Load<Team>("Teams/" + teamName + "/" + teamName);
    }

    /// <summary>
    /// Set flags in tournament main menu 
    /// </summary>
    private void SetFlagsForNextMatch()
    {
        //set local
        localTeamFlag.sprite = teamsForMatch[0].flag;
        localTeamFlag.transform.GetChild(1).GetComponent<Text>().text = teamsForMatch[0].teamName;
        playerFlag.sprite = teamsForMatch[0].flag;
        playerFlag.transform.GetChild(0).GetComponent<Text>().text = teamsForMatch[0].teamName;

        //Set visit
        visitTeamFlag.sprite = teamsForMatch[1].flag;
        visitTeamFlag.transform.GetChild(1).GetComponent<Text>().text = teamsForMatch[1].teamName;
        npcFlag.sprite = teamsForMatch[1].flag;
        npcFlag.transform.GetChild(0).GetComponent<Text>().text = teamsForMatch[1].teamName;
    }

    public void SetNextMatch()
    {
        
    }

    public void ChangePanelAnimation(bool state)
    {
        canvasAnimator.SetBool("NextMatch", state);
    }

    public void ChangeScene(string sceneName)
    {
        Destroy(FindObjectOfType<TournamentController>().gameObject);
        Destroy(FindObjectOfType<MatchInfo>().gameObject);
        SceneManager.LoadScene(sceneName);
    }
}
