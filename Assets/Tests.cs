using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tests : MonoBehaviour
{
    public Transform otherObject;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Vector2.Angle(transform.position, otherObject.position));
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = otherObject.position - transform.position;
        Debug.Log(Vector2.Distance(transform.position, otherObject.position));
        Debug.Log(Vector2.Angle(direction, transform.up));
        Debug.DrawLine(transform.position, otherObject.position, Color.red);
    }
}
