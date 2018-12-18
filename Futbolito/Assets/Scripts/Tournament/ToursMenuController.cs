using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToursMenuController : MonoBehaviour {


    /// Referencia a los torneos (Scripable objects)
    public Tournament[] tours;
    //Referencia al panel que contienen las banderas de los equipos
    public GameObject teamsPanel;

    //Referencia al prefab que contiene la bandera y nombre del equipo.
    public Button teamButton;

    //Array que contiene los botones de los torneos 
    public Button[] toursBtns;
    //Marco vacio y de color para diferencia cuando se selecciona un torneo
    public Sprite selectedTourSprite, notSelectedTourSprite;

    //referencias al layout que le da formato a las imagenes de las banderas cuando se selecciona a un equipo
    public GameObject teamsLayoutObj;
    private GridLayoutGroup teamsLayout;

    //Referencia al mapa del fondo que se colorea segun el torneo seleccionado.
    public Image tourMapSprite;
    public Sprite[] tourMaps;
    

    // Use this for initialization
    void Start () {
        teamsLayout = teamsLayoutObj.GetComponent<GridLayoutGroup>();
	}

    /// <summary>
    /// Metodo para desplegar los equipos que participan en el torneo seleccionado.
    /// En caso de haber ya equipos desplegados, se eliminan.
    /// Posteriormente, se modifican los margenes del layout que contienen a las banderas de los equipos para hacer caber a los equipos de cada torneo.
    /// Por ultimo, se itera y se crean los botones de los equipos segun el torneo seleccionado.
    /// El index indica el torneo seleccionado.
    /// </summary>
    /// <param name="tourIndex"></param>
    public void DisplayTeamsOnPanel(int tourIndex)
    {
        TournamentController._tourCtlr.teamSelected = "";

        DeleteTeamsFromPanel();
        SetTeamsPanel(tours[tourIndex].teams.Length);
        tourMapSprite.sprite = tourMaps[tourIndex];

        Tournament tour = tours[tourIndex];
        for (int i = 0; i < tour.teams.Length; i++)
        {
            Button newTeam = Instantiate(teamButton);
            Team team = tour.teams[i];
            newTeam.image.sprite = team.flag;
            newTeam.GetComponent<TeamSelected>().team = team;
            newTeam.transform.GetChild(0).GetComponent<Text>().text = team.teamName;
            newTeam.transform.SetParent(teamsPanel.transform);
        }
    }

    //Borrar los botones de los equipos del panel.
    void DeleteTeamsFromPanel()
    {
        foreach (Transform child in teamsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    //Al seleccionar algun equipo, un borde aparece alrededor de la bandera.
    public void ChangeButtonSprite(int index) {
        for (int i = 0; i < toursBtns.Length; i++)
        {
            if (index == i) toursBtns[index].image.sprite = selectedTourSprite;
            else toursBtns[i].image.sprite = notSelectedTourSprite;
        }
    }

    //Cambiar los margenes del layout para hacer caber todos los equipos participants de cada torneo.
    void SetTeamsPanel(int teamsN)
    {
        if(teamsN == 16)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(128, 104);
            teamsLayout.spacing = new Vector2(55, 15);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else if (teamsN == 12)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(128, 104);
            teamsLayout.spacing = new Vector2(130, 15);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else if (teamsN == 24)
        {
            teamsLayout.padding = new RectOffset(20, 20, 20, 20);
            teamsLayout.cellSize = new Vector2(104, 80);
            teamsLayout.spacing = new Vector2(11, 45);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else
        {
            teamsLayout.padding = new RectOffset(5,5,20,20);
            teamsLayout.cellSize = new Vector2(80, 56);
            teamsLayout.spacing = new Vector2(10, 70);
            teamsLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
    }

    public void MainMenu(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
