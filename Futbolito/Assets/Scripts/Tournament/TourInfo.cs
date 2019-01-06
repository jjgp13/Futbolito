﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TourInfo{

    public string tourName;
    public string teamSelected;
    public int teamsAmount;
    public int groupsAmount;

    public List<TeamTourInfo> teamList;

    public TourInfo(TournamentController tour)
    {
        tourName = tour.tourName;
        teamSelected = tour.teamSelected;
        teamsAmount = tour.teamsAmount;
        groupsAmount = tour.groupsAmount;
        teamList = tour.teamList;
    }
}