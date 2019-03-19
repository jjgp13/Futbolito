using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {

    //Reference to animator that controls panel that appears if the are an existing tournament.
    public Animator tourExisting;
    
    //Reference to the prefab that contains the tournament info script.
    public GameObject TourController;


    [Header("Reference to shop panel")]
    public Text playerCoins;
    public Text itemName;
    public Image itemImage;
    public Text itemPrice;
    private int itemPriceNumber;

    [Header("Reference to stats player panel")]
    public Text playerTotalMatches;
    public Text playerVictories;
    public Text playerTies;
    public Text playerDefeats;
    public Text playerGoalsScored;
    public Text playerGoalsReceived;
    public Text playerKOVictories;
    public Text playerKODefeats;
    public Text playerEasyLevelMatches;
    public Text playerNormalLevelMatches;
    public Text playerHardLevelMatches;
    public Image playerMostUsedTeamImage;
    public Text playerMostUsedTeamText;
    public Text playerFormationText;
    public Image playerFormationImage;

    private void Start()
    {
        playerCoins.text = PlayerDataController.playerData.playerCoins.ToString();
        SetPlayerStats();
    }

    /// <summary>
    /// this method will load a scene
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    public void LoadScene(string sceneName)
    {
        //This is called in the button New on the panel that appears if there's a previous tournament played.
        if (sceneName == "TournamentSelectionScene") SceneManager.LoadScene(sceneName);

        //This is called in the button Tournament on main menu
        //If there is tournament data already it will show the panel with the option to continue the tournament.
        //If no data, go directly to the tournamnet selection scene.
        if (sceneName == "Tournament")
        {
            //if (SaveSystem.LoadTournament() != null)
            //    tourExisting.SetTrigger("TourExistingIn");
            //else
            //    SceneManager.LoadScene("TournamentSelectionScene");
        }

        //Called by Quick Match button. Load directly Quick match scene.
        if (sceneName == "QuickMatchMenu") SceneManager.LoadScene(sceneName);

        //Called by back button in Continue Tournament panel.
        //It will desapear this panel.
        if (sceneName == "") tourExisting.SetTrigger("TourExistingOut");

        //Called by continue button in Continue tournament panel.
        //This will create the Tournament info object to load Tournament main menu scene with the info.
        if(sceneName == "TourMainMenu")
        {
            TourController.GetComponent<TournamentController>().LoadTour();
            Instantiate(TourController);
            SceneManager.LoadScene(sceneName);
        }
    }
    
    public void ShowPanel(GameObject panel)
    {
        if (panel.activeSelf) panel.SetActive(false);
        else panel.SetActive(true);
    }

    public void SetItemToBuy(ShopItem item)
    {
        itemName.text = item.itemName;
        itemImage.sprite = item.itemImage;
        itemPrice.text = "$ " + item.itemPrice.ToString();
        itemPriceNumber = item.itemPrice;
    }

    public void SetItemImageSize(string item)
    {
        switch (item)
        {
            case "Ball":
                itemImage.rectTransform.sizeDelta = new Vector2(192, 192);
                break;
            case "Uniform":
                itemImage.rectTransform.sizeDelta = new Vector2(128, 256);
                break;
            case "Grass":
                itemImage.rectTransform.sizeDelta = new Vector2(256, 144);
                break;
            case "Table":
                itemImage.rectTransform.sizeDelta = new Vector2(256, 144);
                break;
        }
    }
    
    public void SetPlayerStats()
    {
        PlayerDataController pData = PlayerDataController.playerData;
        playerTotalMatches.text = pData.totalMatches.ToString();
        playerVictories.text = pData.victories.ToString();
        playerTies.text = pData.ties.ToString();
        playerDefeats.text = pData.defeats.ToString();
        playerGoalsScored.text = pData.goalsScored.ToString();
        playerGoalsReceived.text = pData.goalsAgainst.ToString();
        playerKOVictories.text = pData.knockoutVictories.ToString();
        playerKODefeats.text = pData.knockoutDefeats.ToString();
        playerEasyLevelMatches.text = pData.easyLevelMatches.ToString();
        playerNormalLevelMatches.text = pData.normalLevelMatches.ToString();
        playerHardLevelMatches.text = pData.hardLevelMatches.ToString();
        //Flag team most used
        if(pData.teamMostUsed != "")
        {
            playerMostUsedTeamImage.sprite = Resources.Load<Team>("Teams/" + pData.teamMostUsed + "/" + pData.teamMostUsed).flag;
            playerMostUsedTeamText.text = pData.teamMostUsed;
        }
        
        if(pData.mostFormationUsed != "")
        {
            //Text formation
            playerFormationText.text = pData.mostFormationUsed;
            Sprite[] formations = Resources.LoadAll<Sprite>("Formations");
            foreach (var item in formations)
                if (item.name == pData.mostFormationUsed)
                    playerFormationImage.sprite = item;
        }
    }

    public void BuyItem()
    {
        //You have enough money
        if(PlayerDataController.playerData.playerCoins >= itemPriceNumber)
        {
            Debug.Log("Comprado");
        }
        else {
            Debug.Log("No tienes suficiente dinero");
        }
    }
}