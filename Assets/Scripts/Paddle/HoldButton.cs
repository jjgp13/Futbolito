using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour {

    public static HoldButton _holdButton;

    public bool isHolding;
    public float availableTime;
    public Slider holdSlider;
    public bool empty;
    public Button btnReference;

    private void Awake()
    {
        _holdButton = this;
        btnReference = GetComponent<Button>();
    }

    // Use this for initialization
    void Start () {
        availableTime = 3f;
        empty = false;
	}
	
	// Update is called once per frame
	void Update () {

        holdSlider.value = Mathf.Clamp01(availableTime / 3f);

        if (!empty)
        {
            if (isHolding)
                if (availableTime > 0)
                    availableTime -= Time.deltaTime;
                else
                    StartCoroutine(HoldingEmpty());

            if (!isHolding)
                if (availableTime < 3f)
                    availableTime += Time.deltaTime;
        }
    }

    public void Holding()
    {
        isHolding = true;
    }

    public void Release()
    {
        isHolding = false;
    }

    IEnumerator HoldingEmpty()
    {
        empty = true;
        btnReference.interactable = false;
        isHolding = false;
        holdSlider.GetComponent<Animator>().SetBool("Empty", empty);

        yield return new WaitForSeconds(3);

        empty = false;
        btnReference.interactable = true;
        holdSlider.GetComponent<Animator>().SetBool("Empty", empty);
    }
    
}
