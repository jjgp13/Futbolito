using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentController : MonoBehaviour {

    public TourInfo tourInfo;
    private int[] groupsCount = new int[] {4,4,4,4,4,4,4,4};

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        //Si al empezar la escena no encuentra informacion de un torneo. Mostrar pantalla de si desea continuar con la partida previa.
        if(SaveSystem.LoadTournament() != null)
        {
            tourInfo = SaveSystem.LoadTournament();
        }
    }

    public void FillTournamentInfo(string tourName)
    {
        tourInfo.tourName = tourName;
        Tournament tournament = Resources.Load<Tournament>("Tours/"+tourName);
        Debug.Log(tournament);
        tourInfo.teamsAmount = tournament.teams.Length;
        tourInfo.groupsAmount = tournament.teams.Length / 4;
    }

    public void FillTournamentTeamsInfo(Tournament tour)
    {
        List<TeamTourInfo> teams = new List<TeamTourInfo>();

        for (int i = 0; i < tourInfo.teamsAmount; i++)
        {
            Team team = tour.teams[i];
            TeamTourInfo teamTour = new TeamTourInfo(team.name, RandomGroup(),0,0,0,0,0,0);
            teams.Add(teamTour);
        }
    }

    public void TeamSelectedInTour(string ts)
    {
        tourInfo.teamSelected = ts;
    }

    private string RandomGroup()
    {
        string group = "";
        int num;

        do num = Random.Range(0, tourInfo.groupsAmount + 1);
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
}
