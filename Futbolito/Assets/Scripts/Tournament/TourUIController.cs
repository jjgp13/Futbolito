using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TourUIController : MonoBehaviour {

    public GameObject teamInfoUI;
    public GameObject positionsPanel;
    TournamentController tourInfo;

	// Use this for initialization
	void Start () {
        tourInfo = GameObject.Find("TourController").GetComponent<TournamentController>();
        SetTeamsPositionPanel(OrderTeamPositionList(tourInfo.teamList));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetTeamsPositionPanel(List<TeamTourInfo> teamList)
    {
        foreach (TeamTourInfo team in teamList)
        {
            GameObject teamPosition = teamInfoUI;
            Team teamInfo = Resources.Load<Team>("Teams/" + team.name + "/" + team.name);
            teamPosition.transform.GetChild(0).GetComponent<Image>().sprite = teamInfo.flag;
            teamPosition.transform.GetChild(1).GetComponent<Text>().text = team.name;
            Instantiate(teamPosition).transform.SetParent(positionsPanel.transform);
        }
    }

    public List<TeamTourInfo> OrderTeamPositionList(List<TeamTourInfo> teamList)
    {
        List<TeamTourInfo> teamsOrdered = new List<TeamTourInfo>();
        for (int i = 0; i < teamList.Count; i+=4)
        {
            List<TeamTourInfo> groupU = new List<TeamTourInfo>();
            for (int j = i; j < i+4; j++) groupU.Add(teamList[j]);
            List<TeamTourInfo> groupO = groupU.OrderBy(team => team.group).ToList();
            for (int j = 0; j < groupO.Count; j++) teamsOrdered.Add(groupO[j]);
        }

        return teamsOrdered;
    }
}
