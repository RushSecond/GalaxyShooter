using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float mySpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player's posisition to (0,0,0), the "center" of the game world
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the player right by "mySpeed" units, every second
        transform.position += Vector3.right * mySpeed * Time.deltaTime;
    }
}