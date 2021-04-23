using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player's posisition to (0,0,0), the "center" of the game world
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Move the player by "mySpeed" units, every second
        transform.position += Vector3.right * horizontalInput * _mySpeed * Time.deltaTime;
        transform.position += Vector3.up * verticalInput * _mySpeed * Time.deltaTime;
    }
}