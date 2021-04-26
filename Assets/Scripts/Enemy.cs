using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 4f;
    [SerializeField]
    private static float _screenBoundsX = 8.5f;
    [SerializeField]
    private static float _screenBoundsY = 5.4f; 

    void Update()
    {
        transform.Translate(Vector3.down * _mySpeed * Time.deltaTime);

        // if bottom of screen
        // respawn at top with a new random xPosition
        if (transform.position.y <= -1 * _screenBoundsY)
        {
            transform.position = RandomPositionAtTop();
        }
    }

    /// <summary>
    /// Moves the enemy to a random point at the top of the screen
    /// </summary>
    public static Vector3 RandomPositionAtTop()
    {
        float xPosition = Random.Range(-_screenBoundsX, _screenBoundsX);
        float yPosition = _screenBoundsY;
        return new Vector3(xPosition, yPosition, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript)
                playerScript.Damage();
            Destroy(gameObject);
        }

        if (other.tag == "Laser")
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
