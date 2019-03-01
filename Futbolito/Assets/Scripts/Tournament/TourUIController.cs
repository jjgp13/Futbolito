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
    private MatchInfo matchInfo;

    //Auxiliar to change teams in positions panel
    private int teamsInTour;
    private int indexGroupOfteamSelected;
    //Information of the teams for the next match
    //0 is local, 1 is visit
    private Team[] teamsForMatch = new Team[2];

    [Header("Main menu Panel")]
    [Tooltip("Reference to the UI prefab that shows the information of a team in the tournament. ")]
    public GameObject teamTourStatsPrefab;

    //Reference to the panel that show the teams by group.
    public GameObject positionsPanel;
    //Reference to the text of the group. 
    public Text groupText;

    //Reference to buttons. Hide them when tour state is GameOver.
    public Button resultsButton;
    public Button groupsFinalsButton;
    public Text nextMatchText;
    public Text vsGameOverText;
    public Button nextMatchButton;

    //Reference to flags of the next match.
    public Image localTeamFlag;
    public Image visitTeamFlag;
    
    //Reference to animator
    public Animator canvasAnimator;

    //Reference to text in Group/Finals button
    public Text groupFinalsButtonText;

    [Header("Next match panel settings")]
    public Text roundText;

    public Image playerFlag;
    public Image playerFormationImage;
    public Image playerFirstUniform;
    public Image playerSecondUniform;

    public Image npcFlag;
    public Image npcUniform;
    public Text npcFormation;

    public Text matchTimeText;
    public Text matchLevelText;

    [Header("Results panel references")]
    public Text roundTextPrefab;
    public GameObject matchResultPrefab;
    public RectTransform contentResulstPanel;

    [Header("Group/Final panel references")]
    public GameObject groupsPanel;
    public GameObject finalsPanel;
    public Text tourFinalsRound;
    public Image tourFinalsStructure;
    public Image tourCupImage;
    public Sprite round16Structure;
    public Sprite round8Strucure;
    //GameObject that handles the finals
    public Transform leftKeyFinals;
    public Transform rightKeyFinals;
    public Transform final;

    //Singleton
    private void Awake()
    {
        tourInfo = TournamentController._tourCtlr;
        matchInfo = GameObject.FindGameObjectWithTag("MatchData").GetComponent<MatchInfo>();
    }

    // Use this for initialization
    void Start () {

        //Groups stage
        if (tourInfo.matchesRound < 3)
        {
            int roundMatch = tourInfo.matchesRound + 1;
            roundText.text = "Round " + roundMatch.ToString();
            SetInformationForNextMatch();
        }

        //For finals
        if (tourInfo.matchesRound >= 3)
        {
            if (tourInfo.IsPlayerInFinals())
            {
                SetInformationForNextMatch();
                groupsPanel.SetActive(false);
                finalsPanel.SetActive(true);
            }
            else SetGameOverState();
        }

        //Set results panel
        SetResultsPanel();

        //Set finals panel
        SetFinalsPanel();

        //Get teams amount and index group of team selected.
        teamsInTour = tourInfo.teamsAmount;
        indexGroupOfteamSelected = GetTeamGroupSelectedIndex();

        //Set teams in panel.
        //The list was previously order by groups when the tournament was created.
        SetTeamsPositionPanel(OrderTeamsByPoints(tourInfo.teamList));
        ShowTeamsInPanel(indexGroupOfteamSelected);

        groupText.text = "Group " + tourInfo.teamList[indexGroupOfteamSelected].teamGroup;
    }

    /// <summary>
    /// Once groups stage is over, if player's team won't classified to knockout stage,
    /// Tour state is game over.
    /// Hide buttons for next match
    /// </summary>
    private void SetGameOverState()
    {

        //Hide flags and next match button.
        nextMatchText.gameObject.SetActive(false);
        localTeamFlag.gameObject.SetActive(false);
        visitTeamFlag.gameObject.SetActive(false);
        nextMatchButton.gameObject.SetActive(false);

        //Change vsGameOver txt
        vsGameOverText.text = "Game Over";
    }

    /// <summary>
    /// Get the team's list ordeder by group and instatiate prefab with team's info
    /// </summary>
    /// <param name="teamList"></param>
    private void SetTeamsPositionPanel(List<TeamTourInfo> teamList)
    {
        for (int i = 0; i < teamsInTour; i++)
        {
            GameObject teamPosition = teamTourStatsPrefab;
            Team teamInfo = Resources.Load<Team>("Teams/" + teamList[i].teamName + "/" + teamList[i].teamName);
            
            //Set flag
            teamPosition.transform.GetChild(0).GetComponent<Image>().sprite = teamInfo.flag;
            
            //Set name
            teamPosition.transform.GetChild(1).GetComponent<Text>().text = teamList[i].teamName;
            //if the name of the team is equal to the team selected, paint of yellow.
            if (tourInfo.teamSelected == teamList[i].teamName) PaintTeamSelected(teamPosition, Color.yellow);
            else PaintTeamSelected(teamPosition, Color.white);

            //Set matches played
            teamPosition.transform.GetChild(2).GetComponent<Text>().text = tourInfo.matchesRound.ToString();
            
            //Set victories
            teamPosition.transform.GetChild(3).GetComponent<Text>().text = teamList[i].victories.ToString();
            
            // set victories by knockout
            teamPosition.transform.GetChild(4).GetComponent<Text>().text = teamList[i].knockoutVictories.ToString();
            
            //Set drawns
            teamPosition.transform.GetChild(5).GetComponent<Text>().text = teamList[i].draws.ToString();
            
            //Set defeats
            teamPosition.transform.GetChild(6).GetComponent<Text>().text = teamList[i].defeats.ToString();
            
            //set defeats by knockout
            teamPosition.transform.GetChild(7).GetComponent<Text>().text = teamList[i].knockoutDefeats.ToString();
            
            //Set goals for
            teamPosition.transform.GetChild(8).GetComponent<Text>().text = teamList[i].goalsScored.ToString();
            
            // Set goals against
            teamPosition.transform.GetChild(9).GetComponent<Text>().text = teamList[i].goalsReceived.ToString();
            
            //Set goal difference
            int diff = teamList[i].goalsScored - teamList[i].goalsReceived;
            teamPosition.transform.GetChild(10).GetComponent<Text>().text = diff.ToString();
            
            //Set points
            teamPosition.transform.GetChild(11).GetComponent<Text>().text = teamList[i].points.ToString();

            Instantiate(teamPosition).transform.SetParent(positionsPanel.transform);
            positionsPanel.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Instantiate match results prefabs in results panel
    /// </summary>
    private void SetResultsPanel()
    {
        GridLayoutGroup layoutGroup = contentResulstPanel.GetComponent<GridLayoutGroup>();
        switch (tourInfo.teamsAmount)
        {
            case 32:
                layoutGroup.constraintCount = 17;
                break;
            case 16:
                layoutGroup.constraintCount = 9;
                break;
            case 12:
                layoutGroup.constraintCount = 7;
                break;
            case 24:
                layoutGroup.constraintCount = 13;
                break;
        }
        int i = 0;
        while ( i < tourInfo.groupPhaseMatches.Count)
        {
            if (tourInfo.groupPhaseMatches[i].played)
            {
                if (i == 0)
                {
                    Text txt = Instantiate(roundTextPrefab);
                    txt.text = "Round 1";
                    txt.gameObject.transform.SetParent(contentResulstPanel);
                }

                if (i > 0 && tourInfo.groupPhaseMatches[i].matchNumber != tourInfo.groupPhaseMatches[i - 1].matchNumber)
                {
                    Text txt = Instantiate(roundTextPrefab);
                    int r = tourInfo.groupPhaseMatches[i].matchNumber + 1;
                    txt.text = "Round " + r;
                    txt.gameObject.transform.SetParent(contentResulstPanel);
                }

                GameObject mR = matchResultPrefab;
                //Get local team
                Team teamInfo = Resources.Load<Team>("Teams/" + tourInfo.groupPhaseMatches[i].localTeam.teamName + "/" + tourInfo.groupPhaseMatches[i].localTeam.teamName);
                //Set local team flag
                mR.transform.GetChild(0).GetComponent<Image>().sprite = teamInfo.flag;
                //Set local name
                mR.transform.GetChild(1).GetComponent<Text>().text = teamInfo.teamName;
                //Set local goals
                mR.transform.GetChild(2).GetComponent<Text>().text = tourInfo.groupPhaseMatches[i].localGoals.ToString();

                //Set knockoutText
                if (tourInfo.groupPhaseMatches[i].localGoals == 5 || tourInfo.groupPhaseMatches[i].visitGoals == 5)
                    mR.transform.GetChild(4).GetComponent<Text>().text = "Knockout";
                else
                    mR.transform.GetChild(4).GetComponent<Text>().text = "";

                //Get visit team
                teamInfo = Resources.Load<Team>("Teams/" + tourInfo.groupPhaseMatches[i].visitTeam.teamName + "/" + tourInfo.groupPhaseMatches[i].visitTeam.teamName);
                //Set visit team flag
                mR.transform.GetChild(7).GetComponent<Image>().sprite = teamInfo.flag;
                //Set visit name
                mR.transform.GetChild(6).GetComponent<Text>().text = teamInfo.teamName;
                //Set visit goals
                mR.transform.GetChild(5).GetComponent<Text>().text = tourInfo.groupPhaseMatches[i].visitGoals.ToString();

                //Instiate the object as child of content's results panel 
                Instantiate(mR).transform.SetParent(contentResulstPanel);

                //Increase the size of the content's panel
                // contentResulstPanel.rect = new Rect()

                i++;
            } else break;
        };

        //No results yet
        if (i == 0)
        {
            Text txt = Instantiate(roundTextPrefab);
            txt.text = "No results";
            txt.gameObject.transform.SetParent(contentResulstPanel);
        }
    }
    
    /// <summary>
    /// Show and control UI for knockout stage
    /// </summary>
    private void SetFinalsPanel()
    {
        //Set cup image
        Tournament tourCup = Resources.Load<Tournament>("Tours/" + tourInfo.tourName);
        tourCupImage.sprite = tourCup.cupImage;

        int index, limit, matchInfoIndex;
        if (tourInfo.teamsForKnockoutStage == 16)
        {
            index = 0;
            tourFinalsStructure.sprite = round16Structure;
            if (tourInfo.matchesRound == 3)
            {
                limit = 1;
                matchInfoIndex = 0;
            }
            else if (tourInfo.matchesRound == 4)
            {
                limit = 2;
                matchInfoIndex = 4;
            }
            else if (tourInfo.matchesRound == 5)
            {
                limit = 3;
                matchInfoIndex = 6;
            }
            else
            {
                limit = 4;
                matchInfoIndex = 7;
            }
        }
        else
        {
            index = 1;
            tourFinalsStructure.sprite = round8Strucure;
            if (tourInfo.matchesRound == 3)
            {
                limit = 2;
                matchInfoIndex = 2;
            }
            else if (tourInfo.matchesRound == 4)
            {
                limit = 3;
                matchInfoIndex = 3;
            }
            else
            {
                limit = 4;
                matchInfoIndex = 4;
            }
        }
            
        
        if(tourInfo.matchesRound >= 3)
        {
            leftKeyFinals.gameObject.SetActive(true);
            rightKeyFinals.gameObject.SetActive(true);
            for (int i = index; i < limit; i++)
            {
                Transform leftMatchUI = leftKeyFinals.GetChild(i);
                Transform rightMatchUI = rightKeyFinals.GetChild(i);
                leftMatchUI.gameObject.SetActive(true);
                rightMatchUI.gameObject.SetActive(true);

                for (int j = 0; j < leftMatchUI.childCount; j++)
                {
                   //////////////////////////////////////////// //Left key UI
                    Transform matchUI = leftMatchUI.GetChild(j).transform;
                    MatchTourInfo match = tourInfo.leftKeyFinalMatches[matchInfoIndex];

                    Image localFlag = matchUI.GetChild(0).GetComponent<Image>();
                    Image visitFlag = matchUI.GetChild(1).GetComponent<Image>();
                    

                    localFlag.sprite = GetTeamInformation(match.localTeam.teamName).flag;
                    visitFlag.sprite = GetTeamInformation(match.visitTeam.teamName).flag;

                    if (match.played)
                    {
                        if (tourInfo.GetMatchWinnerString(match) == "local") {
                            localFlag.gameObject.SetActive(false);
                            visitFlag.gameObject.SetActive(true);
                        }
                        else
                        {
                            localFlag.gameObject.SetActive(false);
                            visitFlag.gameObject.SetActive(true);
                        }
                    }



                    ///////////////////////////Right key UI
                    matchUI = rightMatchUI.GetChild(j).transform;
                    match = tourInfo.rightKeyFinalMatches[matchInfoIndex];

                    localFlag = matchUI.GetChild(0).GetComponent<Image>();
                    visitFlag = matchUI.GetChild(1).GetComponent<Image>();

                    localFlag.sprite = GetTeamInformation(match.localTeam.teamName).flag;
                    visitFlag.sprite = GetTeamInformation(match.visitTeam.teamName).flag;
                    
                    
                    if (match.played)
                    {
                        if (tourInfo.GetMatchWinnerString(match) == "local")
                        {
                            localFlag.gameObject.SetActive(false);
                            visitFlag.gameObject.SetActive(true);
                        }
                        else
                        {
                            localFlag.gameObject.SetActive(false);
                            visitFlag.gameObject.SetActive(true);
                        }
                    }

                    matchInfoIndex++;
                }
            }
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
            //By points and then by goal difference
            List<TeamTourInfo> groupO = groupU.OrderByDescending(team => team.points).ThenByDescending(team => team.goalDifference).ToList();
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
    private void PaintTeamSelected(GameObject team, Color color)
    {
        Text teamName = team.transform.GetChild(1).GetComponent<Text>();
        teamName.color = color;
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
    /// Set ui panel with next match information and fill MatchInfo object for next match.
    /// </summary>
    private void SetInformationForNextMatch()
    {

        //Local team info
        teamsForMatch[0] = GetTeamInformation(tourInfo.playerMatches[tourInfo.matchesRound].localTeam.teamName);
        //Visit team info
        teamsForMatch[1] = GetTeamInformation(tourInfo.playerMatches[tourInfo.matchesRound].visitTeam.teamName);

        //Set UI
        int player, npc;
        if (teamsForMatch[0].teamName == tourInfo.teamSelected) {
            player = 0;
            npc = 1;
        }
        else {
            player = 1;
            npc = 0;
        }

        //Set flags given index
        localTeamFlag.sprite = teamsForMatch[player].flag;
        localTeamFlag.transform.GetChild(0).GetComponent<Text>().text = teamsForMatch[player].teamName;
        playerFlag.sprite = teamsForMatch[player].flag;
        playerFlag.transform.GetChild(0).GetComponent<Text>().text = teamsForMatch[player].teamName;
        matchInfo.playerTeam = teamsForMatch[player];

        visitTeamFlag.sprite = teamsForMatch[npc].flag;
        visitTeamFlag.transform.GetChild(0).GetComponent<Text>().text = teamsForMatch[npc].teamName;
        npcFlag.sprite = teamsForMatch[npc].flag;
        npcFlag.transform.GetChild(0).GetComponent<Text>().text = teamsForMatch[npc].teamName;
        matchInfo.comTeam = teamsForMatch[npc];



        //Set uniforms
        //Show both for player
        //Local by defaut
        matchInfo.playerUniform = "Local";
        playerFirstUniform.transform.GetChild(0).GetComponent<Image>().sprite = teamsForMatch[player].firstU;
        playerSecondUniform.transform.GetChild(0).GetComponent<Image>().sprite = teamsForMatch[player].secondU;

        //Set a random one to npc
        if (Random.Range(0, 101) < 50)
        {
            npcUniform.transform.GetChild(0).GetComponent<Image>().sprite = teamsForMatch[npc].firstU;
            matchInfo.comUniform = "Local";
        }
        else
        {
            npcUniform.transform.GetChild(0).GetComponent<Image>().sprite = teamsForMatch[npc].secondU;
            matchInfo.comUniform = "Visita";
        }

        //Set npc formation, by defualt.
        playerFormationImage.sprite = teamsForMatch[player].formationImage;
        matchInfo.playerLineUp = teamsForMatch[player].teamFormation;

        npcFormation.text = "Formation " + teamsForMatch[npc].teamFormation.defense.ToString() + "-" +
            teamsForMatch[npc].teamFormation.mid.ToString() + "-" +
            teamsForMatch[npc].teamFormation.attack.ToString();

        matchInfo.comLineUp = teamsForMatch[npc].teamFormation;

        //Set time and level
        matchTimeText.text = "Match time: " + tourInfo.matchTime.ToString() + " min";

        switch (tourInfo.tourLevel)
        {
            case 1:
                matchLevelText.text = "Difficulty: Easy";
                break;
            case 2:
                matchLevelText.text = "Difficulty: Normal";
                break;
            case 3:
                matchLevelText.text = "Difficulty: Hard";
                break;
        }
        
        matchInfo.matchTime = tourInfo.matchTime;
        matchInfo.matchLevel = tourInfo.tourLevel;
    }


    /// <summary>
    /// Control the animation controller in tour main menu panel.
    /// </summary>
    /// <param name="animation">Which animator is going to change</param>
    public void ChangePanelAnimation(string animation)
    {
        bool state = !canvasAnimator.GetBool(animation);
        switch (animation)
        {
            case "NextMatch":
                canvasAnimator.SetBool("NextMatch", state);
                break;
            case "Results":
                canvasAnimator.SetBool("Results", state);
                break;
        }
    }

    /// <summary>
    /// Load scene and make some changes depending on the scene you want to load.
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    public void ChangeScene(string sceneName)
    {
        if(sceneName == "MainMenu")
        {
            //Save tournament data
            tourInfo.SaveTour();
            //Delete object that contains the data.
            Destroy(FindObjectOfType<TournamentController>().gameObject);
            Destroy(FindObjectOfType<MatchInfo>().gameObject);
            SceneManager.LoadScene(sceneName);
        }
        
        if(sceneName == "GameMatch")
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Change views between gropus panel and finals panel.
    /// Called by groups/finals button
    /// </summary>
    public void ShowFinalsPanel()
    {
        if (groupsPanel.activeSelf)
        {
            groupsPanel.SetActive(false);
            finalsPanel.SetActive(true);
        } else
        {
            groupsPanel.SetActive(true);
            finalsPanel.SetActive(false);
        }
    }
}
