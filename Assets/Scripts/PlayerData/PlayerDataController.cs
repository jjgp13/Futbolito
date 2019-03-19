using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerDataController : MonoBehaviour
{

    public static PlayerDataController playerData;

    //Player Stats info
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

    //Player shop info
    public int playerCoins;
    public Dictionary<string, bool> shopItems;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playerData = this;
    }

    public void SavePlayerInfo()
    {
        SaveSystem.SavePlayerData(this);
    }

    public void LoadPlayerData()
    {
        PlayerData data = SaveSystem.LoadPlayerData();

        if(data != null)
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

            playerCoins = data.coins;
            shopItems = data.shopItems;
        }
        else
        {
            CreatePlayerDataFromBeginning();
        }
    }

    /// <summary>
    /// The game object that handles this info will be available from beginning
    /// And across all game scenes;
    /// </summary>
    void Start()
    {
        LoadPlayerData();
    }
    

    /// <summary>
    /// If player has no date yet. Create from zero and save
    /// </summary>
    private void CreatePlayerDataFromBeginning()
    {
        totalMatches = 0;
        victories = 0;
        ties = 0;
        defeats = 0;
        goalsScored = 0;
        goalsAgainst = 0;
        knockoutVictories = 0;
        knockoutDefeats = 0;
        easyLevelMatches = 0;
        normalLevelMatches = 0;
        hardLevelMatches = 0;
        teamMostUsed = "";
        mostFormationUsed = "";

        Dictionary<string, int> teamKeys = new Dictionary<string, int>();
        Dictionary<string, int> formationKeys = new Dictionary<string, int>();

        //Fill Teams dictionary
        Team[] teams = Resources.FindObjectsOfTypeAll<Team>();
        foreach (var team in teams) teamKeys.Add(team.teamName, 0);
        timesTeamSelected = teamKeys;
        
        //Set dictionary with formations
        formationKeys.Add("4-4-2", 0);
        formationKeys.Add("4-3-3", 0);
        formationKeys.Add("4-5-1", 0);
        formationKeys.Add("4-2-4", 0);
        formationKeys.Add("5-3-2", 0);
        formationKeys.Add("5-4-1", 0);
        formationKeys.Add("5-2-3", 0);
        formationKeys.Add("3-4-3", 0);
        formationKeys.Add("3-5-2", 0);
        timesFormationSelected = formationKeys;

        //Information for shop panel
        playerCoins = 0;

        Dictionary<string, bool> shopTemp = new Dictionary<string, bool>();
        ShopItem[] items = Resources.FindObjectsOfTypeAll<ShopItem>();
        foreach (var item in items) shopTemp.Add(item.itemName, false);
        shopItems = shopTemp;

        SavePlayerInfo();
    }

    public void IncrementDictionaryElement(string key, Dictionary<string, int> temp)
    {
        temp[key]++;
    }

    public string GetFirstElementFromDictionaries(Dictionary<string, int> dic)
    {
        Dictionary<string, int> temp = dic;
        temp.OrderBy(key => key.Value);
        return temp.Keys.First();
    }

    public void GetPlayerCoins(string result, int level)
    {
        switch (result)
        {
            case "Victory":
                if (level == 1) playerCoins += 3;
                else if(level == 2) playerCoins += 7;
                else playerCoins += 10;
                break;

            case "Defeat":
                if (level == 1) playerCoins += 1;
                else if (level == 2) playerCoins += 2;
                else playerCoins += 3;
                break;

            case "Tie":
                if (level == 1) playerCoins += 2;
                else if (level == 2) playerCoins += 5;
                else playerCoins += 7;
                break;

        }
    }

}
