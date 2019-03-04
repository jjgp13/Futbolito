using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour {

    //Reference to animator that controls panel that appears if the are an existing tournament.
    public Animator tourExisting;
    
    //Reference to the prefab that contains the tournament info script.
    public GameObject TourController;


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
                tourExisting.SetTrigger("TourExistingIn");
            else
                SceneManager.LoadScene("TournamentSelectionScene");
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

}