using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerData{

    //Stats panel information
    public int totalMatches;
    public int victories;
    public int ties;
    public int defeats;
    public int goalsScored;
    public int goalsAgainst;
    public int knockoutVictories;
    public int knockoutDefeats;
    public int easyLevelMatches;
    public int normalLevelMatches;
    public int hardLevelMatches;
    public string teamMostUsed;
    public string mostFormationUsed;
    public Dictionary<string, int> timesTeamSelected;
    public Dictionary<string, int> timesFormationSelected;

    //Shop panel information
    public int coins;
    public Dictionary<string, bool> shopItems;

    
    public PlayerData(PlayerDataController data)
    {
        totalMatches = data.totalMatches;
        victories = data.victories;
        ties = data.ties;
        defeats = data.defeats;
        goalsScored = data.goalsScored;
        goalsAgainst = data.goalsAgainst;
        knockoutVictories = data.knockoutVictories;
        knockoutDefeats = data.knockoutDefeats;
        easyLevelMatches = data.easyLevelMatches;
        normalLevelMatches = data.normalLevelMatches;
        hardLevelMatches = data.hardLevelMatches;
        teamMostUsed = data.teamMostUsed;
        mostFormationUsed = data.mostFormationUsed;

        coins = data.playerCoins;
    }

}
