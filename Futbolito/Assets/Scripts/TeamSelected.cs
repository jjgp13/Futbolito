using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelected : MonoBehaviour {

    public Image flagOutline;

    public void SelectTeam()
    {
        if(transform.childCount == 0)
        {
            DeletePreviousSelected();
            Image outline = Instantiate(flagOutline);
            outline.transform.SetParent(gameObject.transform);
            outline.transform.position = gameObject.transform.position;
            Vector2 size = gameObject.GetComponent<RectTransform>().sizeDelta;
            int inc = 16;
            outline.rectTransform.sizeDelta = new Vector2(size.x + inc, size.y + inc);
        } else
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }
    }

    void DeletePreviousSelected()
    {
        GameObject parent = gameObject.transform.parent.gameObject;
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).gameObject.transform.childCount > 0)
                Destroy(parent.transform.GetChild(i).GetChild(0).gameObject);
        }
    }
}
