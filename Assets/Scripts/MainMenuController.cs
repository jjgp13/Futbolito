using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour {
    
    //Reference to the prefab that contains the tournament info script.
    public GameObject TourController;
    //Reference to animator that controls panel that appears if the are an existing tournament.
    public  Animator mainMenuAnimator;
    
    [Header("Reference to shop panel")]
    //To make buy animation
    public Animator shopPanelAnimator;
    public Animator coinsAnimator;
    public Text playerCoins;
    public Text itemName;
    public Image itemImage;
    public Text itemPriceText;
    private int itemPrice;

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

    private void Update()
    {
        Debug.Log(string.Format("X:{0} Y:{1} ",Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
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
            if (SaveSystem.LoadTournament() != null)
                mainMenuAnimator.SetBool("ExistingTour", true);
            else
                SceneManager.LoadScene("TournamentSelectionScene");
        }

        //Called by Quick Match button. Load directly Quick match scene.
        if (sceneName == "QuickMatchMenu") SceneManager.LoadScene(sceneName);
        
        //Called by continue button in Continue tournament panel.
        //This will create the Tournament info object to load Tournament main menu scene with the info.
        if(sceneName == "TourMainMenu")
        {
            TourController.GetComponent<TournamentController>().LoadTour();
            Instantiate(TourController);
            SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// called by option panel.
    /// Show or hide.
    /// </summary>
    /// <param name="panel"></param>
    public void ShowPanel(GameObject panel)
    {
        if (panel.activeSelf) panel.SetActive(false);
        else panel.SetActive(true);
    }

    /// <summary>
    /// If there's tour data and player wants to hide continue tour panel
    /// </summary>
    public void HideTourExistingPanel()
    {
        mainMenuAnimator.SetBool("ExisitingTour", false);
    }

    /// <summary>
    /// When a item button is pressed, set image, name and price in central panel.
    /// </summary>
    /// <param name="item"></param>
    public void SetItemToBuy(ShopItem item)
    {
        itemName.text = item.itemName;
        itemImage.sprite = item.itemImage;
        itemPriceText.text = "$ " + item.itemPrice.ToString();
        itemPrice = item.itemPrice;
    }

    /// <summary>
    /// Set main image size in shop panel when a shop item is pressed.
    /// </summary>
    /// <param name="item"></param>
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
    
    /// <summary>
    /// Set player stats when main menu is loaded.
    /// </summary>
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

    /// <summary>
    /// Called from the buy button in shop panel.
    /// </summary>
    public void BuyItem()
    {
        //You have enough money
        if(PlayerDataController.playerData.playerCoins >= itemPrice)
        {
            //Change status on players data dictionary
            PlayerDataController.playerData.shopItems[itemName.text] = true;
            //Decrease amount of coins
            PlayerDataController.playerData.playerCoins -= itemPrice;
            //Play bought animation
            coinsAnimator.SetTrigger("Sell");
            playerCoins.text = PlayerDataController.playerData.playerCoins.ToString();
            //Set shop items list again
            SetShopItems();

            //Save player data
            SaveSystem.SavePlayerData(PlayerDataController.playerData);
        }
        else {
            //Play noth enough money animaiton
            coinsAnimator.SetTrigger("NoMoney");
        }
    }

    IEnumerator DecreasePlayerCoins()
    {
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Set shop panel given the items that player has already bougth
    /// </summary>
    private void SetShopItems()
    {
        foreach (var item in PlayerDataController.playerData.shopItems)
        {
            GameObject itemButton = GameObject.Find(item.Key);
            //if item is buy set button as not interectable.
            if (!item.Value) itemButton.GetComponent<Button>().interactable = true;
            else itemButton.GetComponent<Button>().interactable = false;
        }
    }

}