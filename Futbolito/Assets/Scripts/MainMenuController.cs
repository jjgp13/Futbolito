using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour {

    public Animator tourExisting;
    public GameObject TourController;

    public void LoadScene(string sceneName)
    {
        if (sceneName == "TournamentNew") SceneManager.LoadScene("Tournament");

        if (sceneName == "Tournament")
        {
            if (SaveSystem.LoadTournament() != null) tourExisting.SetTrigger("TourExistingIn");
            else SceneManager.LoadScene(sceneName);
        }

        if (sceneName == "QuickMatchMenu") SceneManager.LoadScene(sceneName);

        if (sceneName == "") tourExisting.SetTrigger("TourExistingOut");

        if(sceneName == "TourMainMenu")
        {
            TournamentController tourObject = TourController.GetComponent<TournamentController>();
            tourObject.LoadTour();
            Instantiate(TourController);
            SceneManager.LoadScene(sceneName);
        }
    }

}