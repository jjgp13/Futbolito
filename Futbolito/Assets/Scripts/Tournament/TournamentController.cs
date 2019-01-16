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
    public List<MatchTourInfo> matchesList;

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
        matchesList = info.matches;
    }
    
    //Este metodo se ejecuta en pantalla de seleccionar torneo.
    //El boton del torneo que es presionado lleva el string del nombre del torneo, que busca en la carpeta de resources.
    //Y llena la informacion de este objeto que es el que se guarda
    public void FillTournamentInfo(string tourNameSelected)
    {
        tourName = tourNameSelected;
        Tournament tournament = Resources.Load<Tournament>("Tours/"+tourNameSelected);
        teamsAmount = tournament.teams.Length;
        groupsAmount = tournament.teams.Length / 4;
    }

    //Tambien se ejecuta cuando se presiona un boton de un torneo.
    //Lleva de parametro el scriptable object del torneo donde contiene la info de los equipos.
    public void FillTournamentTeamsInfo(Tournament tour)
    {
        //Cada que se presiona el boton de un torneo. Limpiar los valores.
        for (int i = 0; i < groupsCount.Length; i++) groupsCount[i] = 4;
        if(teamList.Count > 0) teamList.Clear();

        //Una vez la limpia la lista, volver a llenar la lista de equipos.
        for (int i = 0; i < teamsAmount; i++)
        {
            Team team = tour.teams[i];

            //Se asigna un grupo aleatorio a cada equipo.
            TeamTourInfo teamTour = new TeamTourInfo(team.name, RandomGroup(),0,0,0,0,0,0);
            teamList.Add(teamTour);
        }

        //Se ordena la lista de equipos dado el grupo que se asigno a cada equipo.
        List<TeamTourInfo> sortedList = teamList.OrderBy(team => team.group).ToList();
        teamList = sortedList;
        CreateTourMatches();
    }

    //Hacer el cruce de los partidos de la fase de grupos.
    //6 partidos por grupo. 
    private void CreateTourMatches()
    {
        int[] mNumber = new int[] { 1, 2, 3, 3, 2, 1 };
        int matchesCount = 0;
        for (int i = 0; i < teamsAmount; i+=4)
        {
            int l = i;
            int v = i + 1;
            while (matchesCount < 6)
            {
                MatchTourInfo match = new MatchTourInfo(teamList[l], 0, teamList[v], 0, mNumber[matchesCount]);

                matchesList.Add(match);
                matchesCount++;
                v++;

                if(v == i + 4)
                {
                    l++;
                    v = l + 1;
                }
            }
            matchesCount = 0;
        }
        List<MatchTourInfo> sortedList = matchesList.OrderBy(match => match.matchNumber).ToList();
        matchesList = sortedList;
    }
    

    //Asignar un grupo aleatorio a cada uno de los equipos.
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
