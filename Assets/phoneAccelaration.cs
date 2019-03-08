using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class phoneAccelaration : MonoBehaviour
{
    public Text initialAcc;
    public Text realAccText;
    public Text diffAccText;
    public Vector3 diffAcc;
    public Vector3 iniAcc;

    private void Awake()
    {
        iniAcc = Input.acceleration;
        initialAcc.text = iniAcc.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        print(Camera.main.orthographicSize);
        print(Camera.main.aspect);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        realAccText.text = Input.acceleration.ToString();
        diffAcc = iniAcc - Input.acceleration;
        diffAccText.text = diffAcc.y.ToString();
    }
}
