using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 8f;

    void Update()
    {
        // move right by my speed every second
        transform.Translate(Vector3.up * _mySpeed * Time.deltaTime);

        // if the laser y position is greater than 5.5f, destroy it
        // and destroy parent too if it has one
        if (Mathf.Abs(transform.position.x) > SpawnManager._screenBoundsX)
        {
            Transform myParent = transform.parent;
            if (transform.parent)
            {
                Destroy(myParent.gameObject);
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player
        if (tag == "EnemyLaser" && other.gameObject.tag == "Player")
        {
            Player playerScript = other.GetComponent<Player>();
            HurtPlayer(playerScript);
        }
    }

    void HurtPlayer(Player playerScript)
    {
        if (!playerScript) return;
        playerScript.OnTakeDamage();
        Destroy(gameObject);
    }
}
