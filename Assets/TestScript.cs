using System;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.name = "Mr. Unlimited";
    }

    // public float lat = 43.1140f;
    public float lat = 0f;
    public float lng = -77.5689f;
    public bool moved = false;
    public Vector2 lastmove;
    void FixedUpdate()
    {
        Vector2 previous = new Vector2(lng, lat);
        if(Input.GetKey(KeyCode.W)) {
            lat += (float)0.0005;
        }
        if(Input.GetKey(KeyCode.S)) {
            lat -= (float)0.0005;
        }
        if(Input.GetKey(KeyCode.D)) {
            lng += (float)0.0005;
        }
        if(Input.GetKey(KeyCode.A)) {
            lng -= (float)0.0005;
        }
        Vector2 after = new Vector2(lng, lat);
        if((after-previous).sqrMagnitude > 0) {
            moved = true;
            lastmove = after-previous;
        }
        gameObject.transform.position = new Vector2(lng*100, lat*100);
    }
}
