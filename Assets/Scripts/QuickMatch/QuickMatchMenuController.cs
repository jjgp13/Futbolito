using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuickMatchMenuController : MonoBehaviour {




    public void ChangeScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            Destroy(GameObject.Find("MatchInfo"));
            Destroy(GameObject.FindGameObjectWithTag("PlayerDataObject"));
        }
        SceneManager.LoadScene(sceneName);
    }
}
