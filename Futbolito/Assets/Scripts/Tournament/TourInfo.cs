using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TourInfo{

    public string tourName;
    public string teamSelected;
    public int matchTime;
    public int tourLevel;
    public int teamsAmount;
    public int groupsAmount;
    public int matchesRound;

    public List<TeamTourInfo> teamList;
    public Queue<MatchTourInfo> playerMatches;
    public List<MatchTourInfo> matches;

    public TourInfo(TournamentController tour)
    {
        tourName = tour.tourName;
        teamSelected = tour.teamSelected;
        matchTime = tour.matchTime;
        tourLevel = tour.tourLevel;
        teamsAmount = tour.teamsAmount;
        groupsAmount = tour.groupsAmount;
        matchesRound = tour.matchesRound;
        teamList = tour.teamList;
        matches = tour.matchesList;
    }
}