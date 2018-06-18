using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour {

	
    public void ChangeScene(int indexScene)
    {
        SceneManager.LoadScene(indexScene);
    }
}