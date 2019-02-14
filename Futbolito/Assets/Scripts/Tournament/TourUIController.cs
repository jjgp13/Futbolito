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

    //Reference to the UI prefab that shows the information of a team in the tournament. 
    public GameObject teamTourStatsPrefab;
    //Reference to the panel that show the teams by group.
    public GameObject positionsPanel;
    //Reference to the text of the group. 
    public Text groupText;

    //Info of the tournament
    TournamentController tourInfo;

    private int teamsInTour;
    private int firstTeamActiveInPanel;

    private void Awake()
    {
        tourInfo = TournamentController._tourCtlr;
    }

    // Use this for initialization
    void Start () {
        SetTeamsPositionPanel(OrderTeamPositionList(tourInfo.teamList));
        firstTeamActiveInPanel = 0;
        groupText.text = "Group " + tourInfo.teamList[firstTeamActiveInPanel].group;
    }

    public void SetTeamsPositionPanel(List<TeamTourInfo> teamList)
    {
        for (int i = 0; i < teamsInTour; i++)
        {
            GameObject teamPosition = teamTourStatsPrefab;
            Team teamInfo = Resources.Load<Team>("Teams/" + teamList[i].name + "/" + teamList[i].name);
            teamPosition.transform.GetChild(0).GetComponent<Image>().sprite = teamInfo.flag;
            teamPosition.transform.GetChild(1).GetComponent<Text>().text = teamList[i].name;
            Instantiate(teamPosition).transform.SetParent(positionsPanel.transform);
            teamPosition.SetActive(false);
        }
        ActivateTeamsInPanel(0);
    }

    public List<TeamTourInfo> OrderTeamPositionList(List<TeamTourInfo> teamList)
    {
        List<TeamTourInfo> teamsOrdered = new List<TeamTourInfo>();
        for (int i = 0; i < teamList.Count; i+=4)
        {
            List<TeamTourInfo> groupU = new List<TeamTourInfo>();
            for (int j = i; j < i + 4; j++) groupU.Add(teamList[j]);
            List<TeamTourInfo> groupO = groupU.OrderBy(team => team.group).ToList();
            for (int k = 0; k < groupO.Count; k++) teamsOrdered.Add(groupO[k]);   
        }
        teamsInTour = teamsOrdered.Count;
        return teamsOrdered;
    }

    public void ChangeGroupInPositionPanel(string direction)
    {
        if (direction == "left")
        {
            if (firstTeamActiveInPanel == 0)
            {
                firstTeamActiveInPanel = teamsInTour - 4;
                ActivateTeamsInPanel(firstTeamActiveInPanel);
            }
            else
            {
                firstTeamActiveInPanel -= 4;
                ActivateTeamsInPanel(firstTeamActiveInPanel);
            }
        }
        if (direction == "right")
        {
            if (firstTeamActiveInPanel == teamsInTour - 4)
            {
                firstTeamActiveInPanel = 0;
                ActivateTeamsInPanel(firstTeamActiveInPanel);
            }
            else
            {
                firstTeamActiveInPanel += 4;
                ActivateTeamsInPanel(firstTeamActiveInPanel);
            }
        }
        groupText.text = "Group " + tourInfo.teamList[firstTeamActiveInPanel].group;
    }

    private void ActivateTeamsInPanel(int iniPos)
    {
        for (int i = 0; i < teamsInTour; i++)
        {
            GameObject team = positionsPanel.transform.GetChild(i).gameObject;
            if (i >= firstTeamActiveInPanel && i < firstTeamActiveInPanel + 4) team.SetActive(true);
            else team.SetActive(false);
            PaintTeamSelected(team);
        }
    }

    private void PaintTeamSelected(GameObject team)
    {
        Text teamName = team.transform.GetChild(1).GetComponent<Text>();
        if (teamName.text == tourInfo.teamSelected) teamName.color = Color.yellow;
    }

    public void ChangeScene(string sceneName)
    {
        Destroy(FindObjectOfType<TournamentController>());
        Destroy(FindObjectOfType<MatchInfo>());
        SceneManager.LoadScene(sceneName);
    }
}
