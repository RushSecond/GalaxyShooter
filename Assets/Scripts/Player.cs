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
        CalculateMovement();
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);

        // Move the player by "mySpeed" units, every second
        transform.position += direction * _mySpeed * Time.deltaTime;

        // if player position y is greater than 0
        // set position y to 0
        // if player position y < -4.5
        // set position y to 4.5

        if (transform.position.y > 0)
        {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
        else if (transform.position.y < -4.5f)
        {
            transform.position = new Vector3(transform.position.x, -4.5f, 0);
        }

        // screen wrap on x.
        // if player position x is outside screen bounds -> abs(x) > 9.4f
        // set position x to opposite side of screen -> -9.4f * the sign of the position
        float xPosition = transform.position.x;
        if (Mathf.Abs(xPosition) > 9.4f)
        {
            transform.position = new Vector3(-9.4f * Mathf.Sign(xPosition), transform.position.y, 0);
        }
    }
}