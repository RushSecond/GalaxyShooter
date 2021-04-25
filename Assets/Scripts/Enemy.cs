using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 4f;
    [SerializeField]
    private float _screenBoundsX = 8.5f;
    [SerializeField]
    private float _screenBoundsY = 5.4f; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // move down at 4 meters per second
        transform.Translate(Vector3.down * _mySpeed * Time.deltaTime);

        // if bottom of screen
        // respawn at top with a new random xPosition
        if (transform.position.y <= -1 * _screenBoundsY)
        {
            float xPosition = Random.Range(-_screenBoundsX, _screenBoundsX);
            float yPosition = _screenBoundsY;
            transform.position = new Vector3(xPosition, yPosition, 0);
        }

    }
}
