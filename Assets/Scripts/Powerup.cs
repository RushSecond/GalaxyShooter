using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 3f;

    // Update is called once per frame
    void Update()
    {
        // move down at a speed of 3 (adustable in the inspector)
        transform.Translate(Vector3.down * _mySpeed * Time.deltaTime);

        // if we leave the screen, destroy us
        if (transform.position.y <= -6)
        {
            Destroy(gameObject);
        }
    }

    // On Trigger collision
    // Only collectible by the player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript)
            {
                playerScript.StartTripleShot();
                Destroy(gameObject);
            }
        }
    }
}

