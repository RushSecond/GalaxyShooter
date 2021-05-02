using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 3f;

    void Update()
    {
        transform.Translate(Vector3.down * _mySpeed * Time.deltaTime);

        if (transform.position.y <= -6)
        {
            Destroy(gameObject);
        }
    }

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

