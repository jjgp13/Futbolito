using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    //Lines handlers contains the buttons for shoot, attract and wallPass
    [Header("Reference to team lines handle")]
    public LinesHandler linesHandler;
    public bool isLineActive;

    public string shootButton;
    public string wallPassButton;
    public string magnetButton;

    private void Start()
    {
        linesHandler = transform.parent.GetComponentInParent<LinesHandler>();
        if (linesHandler.numberOfPlayers == 1)
        {
            shootButton = linesHandler.defenseButtons.shootButton;
            magnetButton = linesHandler.defenseButtons.attractButton;
            wallPassButton = linesHandler.defenseButtons.wallPassButton;
        }
        else
        {
            //Controls for two players
            //Think
        }
    }

    public void StopMagnet()
    {
        GetComponentInChildren<PaddleMagnet>().MagnetOff();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isLineActive = IsParentLineActive();
    }

    private bool IsParentLineActive()
    {
        return gameObject.GetComponentInParent<LineMovement>().isActive;
    }

    
}
