using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


/// <summary>
/// This class will handle the information related to the tournament that is being played.
/// Also this class is the one that is saved and loaded in order to recover the tournament.
/// </summary>
public class TournamentController : MonoBehaviour {
    //Singleton
    public static TournamentController _tourCtlr;

    //Tournament info
    public string tourName;
    public string teamSelected;
    public int matchTime;
    public int tourLevel;
    public int teamsAmount;
    public int groupsAmount;
    public int matchesRound;

    public List<TeamTourInfo> teamList;
    public List<MatchTourInfo> playerMatches;
    public List<MatchTourInfo> matchesList;

    //Array that helps to assign randomly a group to each team,
    private int[] groupsCount = new int[] {4,4,4,4,4,4,4,4};

    //When the object is created do not destry it. This object is the information of the tournament
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _tourCtlr = this;
    }

    //Save this tournament
    public void SaveTour()
    {
        SaveSystem.SaveTournament(this);
    }

    //Load this tournament
    public void LoadTour()
    {
        TourInfo info = SaveSystem.LoadTournament();

        tourName = info.tourName;
        teamSelected = info.teamSelected;
        matchTime = info.matchTime;
        tourLevel = info.tourLevel;
        teamsAmount = info.teamsAmount;
        groupsAmount = info.groupsAmount;
        matchesRound = info.matchesRound;

        teamList = info.teamList;
        playerMatches = info.playerMatches;
        matchesList = info.matches;
    }

    /// <summary>
    /// <para>This method fill the info of the tournament.</para>
    /// It is called in TournamentSelectionScene and is attached to the tournament buttons.
    /// This will search the scriptable object of the tournament in the resources folder.
    /// </summary>
    /// <param name="tourNameSelected">Name of the selected tournament</param>
    public void FillTournamentInfo(string tourNameSelected)
    {
        tourName = tourNameSelected;
        Tournament tournament = Resources.Load<Tournament>("Tours/"+tourNameSelected);
        teamsAmount = tournament.teams.Length;
        groupsAmount = tournament.teams.Length / 4;
        //For the playerMatches list, but shows 1 to the user
        matchesRound = 0;
        matchTime = 2;
        tourLevel = 2;
    }

    /// <summary>
    /// <para>This method sets the list of the teams that participate in the tournament.</para>
    /// This is attached to the button of the tournament in TournamentSelectionScene.
    /// </summary>
    /// <param name="tour">Tournament scriptable object</param>
    public void FillTournamentTeamsInfo(Tournament tour)
    {
        //Every time a tournament button is pressed the list is cleared and the groups array is refilled.
        for (int i = 0; i < groupsCount.Length; i++) groupsCount[i] = 4;
        if(teamList.Count > 0) teamList.Clear();

        //Iterate over the teams in the tournament and filled its information related to this new tournament created.
        for (int i = 0; i < teamsAmount; i++)
        {
            Team team = tour.teams[i];

            //Create the team's info for this tournament.
            //Assign a random group in the tour and add it to the list.
            TeamTourInfo teamTour = new TeamTourInfo(team.name, RandomGroup(),0,0,0,0,0,0,0,0);
            teamList.Add(teamTour);
        }

        //Order the team's list by group
        List<TeamTourInfo> sortedList = teamList.OrderBy(team => team.teamGroup).ToList();
        teamList = sortedList;
        //Create the matches of the group phase of the tournament.
        CreateTourMatches();
    }

    /// <summary>
    /// <para>This method create the match of the tournament of the group phase.</para>
    /// <para>Each group has 6 matches (Groups of 4)</para>
    /// </summary>
    private void CreateTourMatches()
    {
        //pattern to assign a number of match in the group
        int[] mNumber = new int[] { 1, 2, 3, 3, 2, 1 };
        int matchesCount = 0;
        for (int i = 0; i < teamsAmount; i+=4)
        {
            int l = i;
            int v = i + 1;
            //Create 6 matches
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
        //Order the list of matches by match number. 2 matches for group every round.
        List<MatchTourInfo> sortedList = matchesList.OrderBy(match => match.matchNumber).ToList();
        matchesList = sortedList;
    }
    
    public void GetPlayerMatches()
    {
        playerMatches.Clear();
        //Get matches of the player.
        for (int i = 0; i < matchesList.Count; i++)
        {
            //if the match created has the team selected, add match to player matches.
            if (matchesList[i].localTeam.teamName == teamSelected || matchesList[i].visitTeam.teamName == teamSelected)
            {
                MatchTourInfo match = new MatchTourInfo(matchesList[i]);
                playerMatches.Add(match);
            }
        }
    }

    private MatchTourInfo SimulateMatch(MatchTourInfo match)
    {
        int localGoals = Random.Range(0, 6);
        int visitGoals;

        if (localGoals == 5)
            visitGoals = Random.Range(0, 5);
        else
            visitGoals = Random.Range(0, 6);

        match.localGoals = localGoals;
        match.visitGoals = visitGoals;
        return new MatchTourInfo(match);
    }

    private void UpdateTeamInformation(MatchTourInfo match)
    {
        //Local in 0 and visit in 1
        int localTeamIndex = GetTeamIndex(match.localTeam.teamName);
        int visitTeamIndex = GetTeamIndex(match.visitTeam.teamName);

        //Update goals
        teamList[localTeamIndex].goalsScored += match.localGoals;
        teamList[localTeamIndex].goalsReceived += match.visitGoals;

        teamList[visitTeamIndex].goalsScored += match.visitGoals;
        teamList[visitTeamIndex].goalsReceived += match.localGoals;

        //Update Victories, defeats, draws and points
        if (match.localGoals > match.visitGoals)
        {
            teamList[localTeamIndex].victories++;
            teamList[localTeamIndex].points += 3;
            teamList[visitTeamIndex].defeats++;
        } else if (match.localGoals < match.visitGoals)
        {
            teamList[localTeamIndex].defeats++;
            teamList[visitTeamIndex].victories++;
            teamList[visitTeamIndex].points += 3;
        }
        else
        {
            teamList[localTeamIndex].draws++;
            teamList[localTeamIndex].points++;
            teamList[visitTeamIndex].draws++;
            teamList[visitTeamIndex].points++;
        }

        //Update knockout victories and defeats.
        if(match.localGoals == 5)
        {
            teamList[localTeamIndex].knockoutVictories++;
            teamList[visitTeamIndex].knockoutDefeats++;
        }
        if (match.visitGoals == 5)
        {
            teamList[localTeamIndex].knockoutDefeats++;
            teamList[visitTeamIndex].knockoutVictories++;
        }
    }

    private int GetTeamIndex(string team)
    {
        for (int i = 0; i < teamList.Count; i++)
        {
            if (team == teamList[i].teamName) return i;
        }
        return 0;
    }



    public void SimulateRoundOfMatches(int round)
    {
        for (int i = 0; i < matchesList.Count; i++)
        {
            if (matchesList[i].matchNumber == round && matchesList[i].localTeam.teamName != teamSelected && matchesList[i].visitTeam.teamName != teamSelected)
            {
                matchesList[i] = SimulateMatch(matchesList[i]);
                UpdateTeamInformation(matchesList[i]);
            }
        }

        matchesRound++;
    }

    /// <summary>
    /// This method assigns a group to each team.
    /// </summary>
    /// <returns>The latter of the group</returns>
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
}
