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
    public Dictionary<string, int> timesTeamSelected = new Dictionary<string, int>();
    public Dictionary<string, int> timesFormationSelected = new Dictionary<string, int>();

    //Player shop info
    public int playerCoins;
    public Dictionary<string, bool> shopItems;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playerData = this;
    }

    /// <summary>
    /// The game object that handles this info will be available from beginning
    /// And across all game scenes;
    /// </summary>
    void Start()
    {
        LoadPlayerData();
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
            //Player stats
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
            timesTeamSelected = data.timesTeamSelected;
            timesFormationSelected = data.timesFormationSelected;
            //For shop
            playerCoins = data.coins;
            shopItems = data.shopItems;
        }
        else
        {
            CreatePlayerDataFromBeginning();
        }
    }
    

    /// <summary>
    /// If player has no data yet. Create from zero and save
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
        Team[] teams = Resources.LoadAll("Teams", typeof(Team)).Cast<Team>().ToArray();
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
        ShopItem[] items = Resources.LoadAll("", typeof(ShopItem)).Cast<ShopItem>().ToArray();
        foreach (var item in items) shopTemp.Add(item.itemName, false);
        shopItems = shopTemp;

        SavePlayerInfo();
    }

    /// <summary>
    /// To handle player stats. If an action is triggered. Increment the key of the dictionary with player's information.
    /// </summary>
    /// <param name="key">Key to increment</param>
    /// <param name="temp">Dictionary in which key is present.</param>
    public void IncrementDictionaryElement(string key, Dictionary<string, int> temp)
    {
        temp[key]++;
    }

    /// <summary>
    /// Order a dictonary and get first element
    /// </summary>
    /// <param name="dic">Dictionary to order</param>
    /// <returns>Sring with firs key element alphabetic ordered</returns>
    public string GetFirstElementFromDictionaries(Dictionary<string, int> dic)
    {
        Dictionary<string, int> temp = dic;
        temp.OrderBy(key => key.Value);
        return temp.Keys.First();
    }

    /// <summary>
    /// Calculate how many coint get a player given the result and the match level
    /// </summary>
    /// <param name="result">Match result(Victory, Defeat, Tie)</param>
    /// <param name="level">Match level (Easy=1, Normal=2, Hard=3)</param>
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
