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
    public int teamsForKnockoutStage;
    public List<TeamTourInfo> teamList;
    public List<MatchTourInfo> playerMatches;
    public List<MatchTourInfo> groupPhaseMatches;

    //Information for finals
    public List<TeamTourInfo> teamsForFinals;
    public List<MatchTourInfo> leftKeyFinalMatches;
    public List<MatchTourInfo> rightKeyFinalMatches;
    public MatchTourInfo finalMatch;

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
        teamsForKnockoutStage = info.teamsForKnockoutStage;

        teamList = info.teamList;
        playerMatches = info.playerMatches;
        groupPhaseMatches = info.groupPhaseMatches;

        teamsForFinals = info.teamsForFinals;
        leftKeyFinalMatches = info.leftKeyFinalMatches;
        rightKeyFinalMatches = info.rightKeyFinalMatches;

        finalMatch = info.finalMatch;
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

        //Get teams for knockout stage.
        switch (teamsAmount)
        {
            //World cup
            case 32:
                teamsForKnockoutStage = 16;
                break;
            //American, Asia, African Cup
            case 16:
                teamsForKnockoutStage = 8;
                break;
            //Gold Cup
            case 12:
                teamsForKnockoutStage = 8;
                break;
            //Euro cup
            case 24:
                teamsForKnockoutStage = 16;
                break;
        }
    }

    /// <summary>
    /// <para>This method sets the list of the teams that participate in the tournament.</para>
    /// This is attached to the button of the tournament in TournamentSelectionScene.
    /// </summary>
    /// <param name="tour">Tournament scriptable object</param>
    public void FillTournamentTeamsInfo(Tournament tour)
    {
        //Clear matche's list
        teamList.Clear();
        playerMatches.Clear();
        groupPhaseMatches.Clear();
        //Every time a tournament button is pressed the list is cleared and the groups array is refilled.
        for (int i = 0; i < groupsCount.Length; i++) groupsCount[i] = 4;

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
        CreateGroupPhaseMatches();
    }

    /// <summary>
    /// <para>This method create the match of the tournament of the group phase.</para>
    /// <para>Each group has 6 matches (Groups of 4)</para>
    /// </summary>
    private void CreateGroupPhaseMatches()
    {
        //pattern to assign a number of match in the group
        int[] mNumber = new int[] { 0, 1, 2, 2, 1, 0 };
        int matchesCount = 0;
        for (int i = 0; i < teamsAmount; i+=4)
        {
            int l = i;
            int v = i + 1;
            //Create 6 matches
            while (matchesCount < 6)
            {
                MatchTourInfo match = new MatchTourInfo(teamList[l], 0, teamList[v], 0, mNumber[matchesCount], false);

                groupPhaseMatches.Add(match);
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
        List<MatchTourInfo> sortedList = groupPhaseMatches.OrderBy(match => match.matchNumber).ToList();
        groupPhaseMatches = sortedList;
    }
    
    /// <summary>
    /// When a tournament button is pressed. The phase group matches are created.
    /// After that, this method search in matches list and add the player's to this list.
    /// </summary>
    public void GetPlayerMatchesInGroupPhase()
    {
        playerMatches.Clear();
        //Itarate over group matches list and get player matches
        for (int i = 0; i < groupPhaseMatches.Count; i++)
            if(IsPlayerMatch(groupPhaseMatches[i])) playerMatches.Add(groupPhaseMatches[i]);
    }

    /// <summary>
    /// Returns player's match with result got it from match controller.
    /// </summary>
    /// <param name="match">Player's match with no info</param>
    /// <returns>Player's match with new result</returns>
    private MatchTourInfo GetPlayerMatchResult(MatchTourInfo match)
    {
        if (match.localTeam.teamName == teamSelected)
        {
            match.localGoals = MatchScoreController.instance.LeftTeamScore;
            match.visitGoals = MatchScoreController.instance.RightTeamScore;
        }
        else
        {
            match.localGoals = MatchScoreController.instance.RightTeamScore;
            match.visitGoals = MatchScoreController.instance.LeftTeamScore;
        }

        match.played = true;

        return new MatchTourInfo(match);
    }

    /// <summary>
    /// Simulate a match between npc's getting match result with random numbers
    /// </summary>
    /// <param name="match">Npc's match with no info</param>
    /// <returns>Npc's match with new result</returns>
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
        match.played = true;
        return new MatchTourInfo(match);
    }

    /// <summary>
    /// Simulate a npc match but cannot be a draw in the match
    /// For final matches.
    /// </summary>
    /// <param name="match">Npc's match with no info</param>
    /// <returns>Npc's match with new result</returns>
    private MatchTourInfo SimulateMatchNoDraws(MatchTourInfo match)
    {
        int localGoals = 0;
        int visitGoals = 0;
        while (localGoals == visitGoals)
        {
            localGoals = Random.Range(0, 6);
            visitGoals = Random.Range(0, 6);
        }

        match.localGoals = localGoals;
        match.visitGoals = visitGoals;
        match.played = true;
        return new MatchTourInfo(match);
    }

    /// <summary>
    /// Return a winner of a match comparing scores.
    /// </summary>
    /// <param name="match">Match which want to be evaluate</param>
    /// <returns>String with name of the winner</returns>
    public TeamTourInfo GetMatchWinnerTeamInfo(MatchTourInfo match)
    {
        if (match.localGoals > match.visitGoals)
            return match.localTeam;
        else
            return match.visitTeam;
    }

    public string GetMatchWinnerString(MatchTourInfo match)
    {
        if (match.localGoals > match.visitGoals)
            return "local";
        else
            return "visit";
    }

    /// <summary>
    /// Get result from a match and look for teams that have played and update team tour information.
    /// </summary>
    /// <param name="match">Match with result information</param>
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

    /// <summary>
    /// Get team selected index from team's list.
    /// </summary>
    /// <param name="team">team selected</param>
    /// <returns>The index of the first team in player's group</returns>
    private int GetTeamIndex(string team)
    {
        for (int i = 0; i < teamList.Count; i++)
        {
            if (team == teamList[i].teamName) return i;
        }
        return 0;
    }

    /// <summary>
    /// Iterate over matches list and simulate result for each match.
    /// if it's a player's match get the result from match controller.
    /// At the end, increase the round of the tournament.
    /// </summary>
    /// <param name="round">Playing round</param>
    public void SimulateRoundOfMatches()
    {
        for (int i = 0; i < groupPhaseMatches.Count; i++)
        {
            if (matchesRound == groupPhaseMatches[i].matchNumber)
            {
                //NPC match
                if (!IsPlayerMatch(groupPhaseMatches[i]))
                {
                    groupPhaseMatches[i] = SimulateMatch(groupPhaseMatches[i]);
                    UpdateTeamInformation(groupPhaseMatches[i]);
                }
                //PlayerMatch
                if (IsPlayerMatch(groupPhaseMatches[i]))
                {
                    playerMatches[matchesRound] = GetPlayerMatchResult(playerMatches[matchesRound]);
                    groupPhaseMatches[i] = GetPlayerMatchResult(groupPhaseMatches[i]);
                    UpdateTeamInformation(groupPhaseMatches[i]);
                }
            }
        }
        matchesRound++;

        if (matchesRound == 3)
        {
            GetFinalTeams(teamsForKnockoutStage);
            SetKnockoutStageMatches();
        }
    }

    /// <summary>
    /// Itarate over list of finals matches and get a result.
    /// Create new matches for stage and add them at the end of final matches lists
    /// </summary>
    /// <param name="round">Match round</param>
    public void SimulateRoundOfMatchesInKnockOutStage()
    {
        //Final
        if (teamsForKnockoutStage == 16 && matchesRound == 6)
        {
            finalMatch = GetPlayerMatchResult(finalMatch);
            playerMatches[matchesRound] = GetPlayerMatchResult(playerMatches[matchesRound]);
            return;
        }
        if (teamsForKnockoutStage == 8 && matchesRound == 5)
        {
            finalMatch = GetPlayerMatchResult(finalMatch);
            playerMatches[matchesRound] = GetPlayerMatchResult(playerMatches[matchesRound]);
            return;
        }
        
        //Simulate matches of one round
        for (int i = 0; i < leftKeyFinalMatches.Count; i++)
        {
            MatchTourInfo leftMatch = leftKeyFinalMatches[i];
            if (leftMatch.matchNumber == matchesRound)
            {
                if (!IsPlayerMatch(leftMatch))
                {
                    if (!leftMatch.played) leftMatch = SimulateMatchNoDraws(leftMatch);
                }
                else
                {
                    playerMatches[matchesRound] = GetPlayerMatchResult(playerMatches[matchesRound]);
                    leftMatch = GetPlayerMatchResult(leftMatch);
                }
            }
            
            MatchTourInfo rightMatch = rightKeyFinalMatches[i];
            if(rightMatch.matchNumber == matchesRound)
            {
                if (!IsPlayerMatch(rightMatch))
                {
                    if (!rightMatch.played) rightMatch = SimulateMatchNoDraws(rightMatch);
                }
                else
                {
                    playerMatches[matchesRound] = GetPlayerMatchResult(playerMatches[matchesRound]);
                    rightMatch = GetPlayerMatchResult(rightMatch);
                }
            }
        }
        matchesRound++;

        //Create next matches
        if(teamsForKnockoutStage == 16)
        {
            switch (matchesRound)
            {
                //Create QuarteFinals
                case 4:
                    CreateNextRoundInFinals(0, 4);
                    break;
                //Create semifinals
                case 5:
                    CreateNextRoundInFinals(4, 6);
                    break;
                case 6:
                    CreateFinalMatch();
                    break;
            }
        }
        else
        {
            switch (matchesRound)
            {
                //Create semifinals
                case 4:
                    CreateNextRoundInFinals(0, 2);
                    break;
                case 5:
                    CreateFinalMatch();
                    break;
            }
        }
    }

    /// <summary>
    /// Get winner of both key and create final match
    /// </summary>
    private void CreateFinalMatch()
    {
        TeamTourInfo leftKeyWinner = GetMatchWinnerTeamInfo(leftKeyFinalMatches[leftKeyFinalMatches.Count-1]);
        TeamTourInfo rightKeyWinner = GetMatchWinnerTeamInfo(rightKeyFinalMatches[rightKeyFinalMatches.Count - 1]);
        MatchTourInfo final = new MatchTourInfo(leftKeyWinner, 0, rightKeyWinner, 0, matchesRound, false);
        finalMatch = final;
        if (IsPlayerMatch(final)) playerMatches.Add(final);
    }
    
    /// <summary>
    /// Create matches for next final round
    /// </summary>
    /// <param name="startIndex">From which match is goinf to take winner</param>
    /// <param name="endIndex">Where list of winner end</param>
    private void CreateNextRoundInFinals(int startIndex, int endIndex)
    {
        List<TeamTourInfo> winnersLeftKey = new List<TeamTourInfo>();
        List<TeamTourInfo> winnersRightKey = new List<TeamTourInfo>();
        for (int i = startIndex; i < endIndex; i++)
        {
            winnersLeftKey.Add(GetMatchWinnerTeamInfo(leftKeyFinalMatches[i]));
            winnersRightKey.Add(GetMatchWinnerTeamInfo(rightKeyFinalMatches[i]));
        }
        for (int i = 0; i < winnersLeftKey.Count; i+=2)
        {
            MatchTourInfo leftmatch = new MatchTourInfo(winnersLeftKey[i], 0, winnersLeftKey[i + 1], 0, matchesRound, false);
            leftKeyFinalMatches.Add(leftmatch);
            if (IsPlayerMatch(leftmatch)) playerMatches.Add(leftmatch);

            MatchTourInfo rightMatch = new MatchTourInfo(winnersRightKey[i], 0, winnersRightKey[i + 1], 0, matchesRound, false);
            rightKeyFinalMatches.Add(rightMatch);
            if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);
        }
    }

    /// <summary>
    /// Create finals matches given the tournament and the number of the teams in.
    /// </summary>
    public void SetKnockoutStageMatches()
    {
        switch (teamsAmount)
        {
            //World cup
            case 32:
                CreateFinalMatchesFor32and16Teams();
                break;
            //American, Asia, African Cup
            case 16:
                CreateFinalMatchesFor32and16Teams();
                break;
            //Gold Cup
            case 12:
                CreateFinalMatchesFor12Teams();
                break;
            //Euro cup
            case 24:
                CreateFinalMatchesFor24Teams();
                break;
        }
        
    }

    /// <summary>
    /// Check if team selected is in knockout stage
    /// </summary>
    /// <returns>True if it is, false if not</returns>
    public bool IsPlayerInFinals()
    {
        if(matchesRound == 3)
        {
            foreach (var team in teamsForFinals)
                if (team.teamName == teamSelected) return true;
        }

        if(matchesRound > 3)
        {
            for (int i = 0; i < leftKeyFinalMatches.Count; i++)
            {
                MatchTourInfo match = leftKeyFinalMatches[i];
                if(match.matchNumber == matchesRound-1)
                {
                    if (match.played)
                        if (GetMatchWinnerTeamInfo(match).teamName == teamSelected) return true;

                    match = rightKeyFinalMatches[i];
                    if (match.played)
                        if (GetMatchWinnerTeamInfo(match).teamName == teamSelected) return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Create the finals matches for world cup, Amercian Cup, Asia and Africa.
    /// Since they have the same pattern of combination in knockout stage
    /// </summary>
    private void CreateFinalMatchesFor32and16Teams()
    {
        for (int i = 0; i < teamsForFinals.Count; i += 4)
        {
            //Left key
            MatchTourInfo leftMatch = new MatchTourInfo(teamsForFinals[i], 0, teamsForFinals[i + 3], 0, 3, false);
            leftKeyFinalMatches.Add(leftMatch);
            if(IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);

            //Right key
            MatchTourInfo rightMatch = new MatchTourInfo(teamsForFinals[i + 1], 0, teamsForFinals[i + 2], 0, 3, false);
            rightKeyFinalMatches.Add(rightMatch);
            if(IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);
        }
    }

    /// <summary>
    /// Create finals for gold cup
    /// </summary>
    private void CreateFinalMatchesFor12Teams()
    {
        //Left key 
        //match 1
        MatchTourInfo rightMatch;
        MatchTourInfo leftMatch = new MatchTourInfo(teamsForFinals[0], 0, teamsForFinals[3], 0, 3, false);
        leftKeyFinalMatches.Add(leftMatch);
        if (IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);
        //match2
        if (teamsForFinals[6].teamGroup == "A" || teamsForFinals[6].teamGroup == "B")
        {
            leftMatch = new MatchTourInfo(teamsForFinals[2], 0, teamsForFinals[6], 0, 3, false);
            rightMatch = new MatchTourInfo(teamsForFinals[4], 0, teamsForFinals[7], 0, 3, false);
        }
        else
        {
            leftMatch = new MatchTourInfo(teamsForFinals[2], 0, teamsForFinals[7], 0, 3, false);
            rightMatch = new MatchTourInfo(teamsForFinals[4], 0, teamsForFinals[6], 0, 3, false);
        }
        leftKeyFinalMatches.Add(leftMatch);
        if (IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);
        
        //Right key
        //Match 3
        rightKeyFinalMatches.Add(rightMatch);
        if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);
        
        //Match 4
        rightMatch = new MatchTourInfo(teamsForFinals[5], 0, teamsForFinals[1], 0, 3, false);
        rightKeyFinalMatches.Add(rightMatch);
        if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);
    }

    /// <summary>
    /// Create finals for EuroCup
    /// </summary>
    private void CreateFinalMatchesFor24Teams()
    {
        MatchTourInfo leftMatch, rightMatch;
        //Left key matches
        //Match 1
        leftMatch = new MatchTourInfo(teamsForFinals[1], 0, teamsForFinals[5], 0, 3, false);
        leftKeyFinalMatches.Add(leftMatch);
        if (IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);

        //Match 2
        leftMatch = new MatchTourInfo(teamsForFinals[6], 0, teamsForFinals[15], 0, 3, false);
        leftKeyFinalMatches.Add(leftMatch);
        if (IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);

        //Match 3
        leftMatch = new MatchTourInfo(teamsForFinals[2], 0, teamsForFinals[14], 0, 3, false);
        leftKeyFinalMatches.Add(leftMatch);
        if (IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);

        //Match 3
        leftMatch = new MatchTourInfo(teamsForFinals[10], 0, teamsForFinals[9], 0, 3, false);
        leftKeyFinalMatches.Add(leftMatch);
        if (IsPlayerMatch(leftMatch)) playerMatches.Add(leftMatch);

        //Right key matches
        //Match 1
        rightMatch = new MatchTourInfo(teamsForFinals[4], 0, teamsForFinals[13], 0, 3, false);
        rightKeyFinalMatches.Add(rightMatch);
        if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);

        //Match 2
        rightMatch = new MatchTourInfo(teamsForFinals[8], 0, teamsForFinals[7], 0, 3, false);
        rightKeyFinalMatches.Add(rightMatch);
        if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);

        //Match 3
        rightMatch = new MatchTourInfo(teamsForFinals[0], 0, teamsForFinals[12], 0, 3, false);
        rightKeyFinalMatches.Add(rightMatch);
        if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);

        //Match 4
        rightMatch = new MatchTourInfo(teamsForFinals[3], 0, teamsForFinals[11], 0, 3, false);
        rightKeyFinalMatches.Add(rightMatch);
        if (IsPlayerMatch(rightMatch)) playerMatches.Add(rightMatch);
    }

    /// <summary>
    /// Checks if a match has the player team, if so adds it to player's list matches 
    /// </summary>
    /// <param name="match">Match to check</param>
    private bool IsPlayerMatch(MatchTourInfo match)
    {
        if (match.localTeam.teamName == teamSelected || match.visitTeam.teamName == teamSelected) return true;
        else return false;
    }

    /// <summary>
    /// Iterate over players list 
    /// </summary>
    /// <param name="finalTeams"></param>
    public void GetFinalTeams(int finalTeams)
    {
        //If necesary
        Queue<TeamTourInfo> thirdPlaces = new Queue<TeamTourInfo>();
        //Groups unorderer and orderer
        List<TeamTourInfo> groupU = new List<TeamTourInfo>();
        List<TeamTourInfo> groupO = new List<TeamTourInfo>();

        for (int i = 0; i < teamList.Count; i += 4)
        {
            for (int j = i; j < i + 4; j++) groupU.Add(teamList[j]);
            //By points and then by goal difference
            groupO = groupU.OrderByDescending(team => team.points).ThenByDescending(team => team.goalDifference).ToList();
            //Get first 2 of each group
            teamsForFinals.Add(groupO[0]);
            teamsForFinals.Add(groupO[1]);
            thirdPlaces.Enqueue(groupO[2]);

            //Clear lists
            groupU.Clear();
            groupO.Clear();
        }
        //Order by point and goal difference to get best 3rd places.
        Queue<TeamTourInfo> ordererthirdPlaces = 
            new Queue<TeamTourInfo>(thirdPlaces.OrderByDescending(team => team.points).ThenByDescending(team => team.goalDifference));
        
        //If gold cup or euro cup
        //Get best 3rd places.
        if (teamsAmount == 12 || teamsAmount == 24)
        {
            //If 12 or 24 teams in tournament get 3rd place for each group
            while (teamsForFinals.Count < finalTeams)
                teamsForFinals.Add(ordererthirdPlaces.Dequeue());
        }
    }

    /// <summary>
    /// Get Scriptable object with team's information
    /// </summary>
    /// <param name="team">Name of the team</param>
    /// <returns>Team's information Scriptable object.</returns>
    public Team LoadTeamInformation(string team)
    {
        Team teamInfo = Resources.Load<Team>("Teams/" + team + "/" + team);
        return teamInfo;
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
