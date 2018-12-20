using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class TournamentController : MonoBehaviour {

    public static TournamentController _tourCtlr;

    public string tourName;
    public string teamSelected;
    public int teamsAmount;
    public int groupsAmount;
    public List<TeamTourInfo> teamList;

    private int[] groupsCount = new int[] {4,4,4,4,4,4,4,4};

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _tourCtlr = this;
    }

    public void SaveTour()
    {
        SaveSystem.SaveTournament(this);
    }

    public void LoadTour()
    {
        TourInfo info = SaveSystem.LoadTournament();

        tourName = info.tourName;
        teamSelected = info.teamSelected;
        teamsAmount = info.teamsAmount;
        groupsAmount = info.groupsAmount;

        teamList = info.teamList;
    }

    public void FillTournamentInfo(string tourNameSelected)
    {
        tourName = tourNameSelected;
        Tournament tournament = Resources.Load<Tournament>("Tours/"+tourNameSelected);
        teamsAmount = tournament.teams.Length;
        groupsAmount = tournament.teams.Length / 4;
    }

    public void FillTournamentTeamsInfo(Tournament tour)
    {
        //Cada que se presiona el boton de un torneo. Limpiar los valores.
        for (int i = 0; i < groupsCount.Length; i++) groupsCount[i] = 4;
        if(teamList.Count > 0) teamList.Clear();

        //Una vez la limpia la lista, volver a llenar la lista de equipos.
        for (int i = 0; i < teamsAmount; i++)
        {
            Team team = tour.teams[i];
            TeamTourInfo teamTour = new TeamTourInfo(team.name, RandomGroup(),0,0,0,0,0,0);
            
            teamList.Add(teamTour);
        }
        List<TeamTourInfo> sortedList = teamList.OrderBy(team => team.group).ToList();
        teamList = sortedList;
    }

    private string RandomGroup()
    {
        string group = "";
        int num;

        do num = Random.Range(0, groupsAmount);
        while (groupsCount[num] == 0);

        groupsCount[num]--;

        switch (num)
        {
            case 0:
                group = "A";
                break;
            case 1:
                group = "B";
                break;
            case 2:
                group = "C";
                break;
            case 3:
                group = "D";
                break;
            case 4:
                group = "E";
                break;
            case 5:
                group = "F";
                break;
            case 6:
                group = "G";
                break;
            case 7:
                group = "H";
                break;
        }
        return group;
    }

    public void StartTournament(string sceneName)
    {
        SaveTour();
        SceneManager.LoadScene(sceneName);
    }
}
