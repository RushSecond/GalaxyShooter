using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour, IProjectile
{
    [SerializeField]
    private float _mySpeed = 8f;
    [SerializeField]
    int _damage = 1;

    public int GetDamage()
    {
        return _damage;
    }

    void Update()
    {
        // move right by my speed every second
        transform.Translate(Vector3.up * _mySpeed * Time.deltaTime);

        // if the laser y position is greater than 5.5f, destroy it
        // and destroy parent too if it has one
        if (!CameraManager.IsInsideCameraBounds(transform.position, 2f))
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
            PlayerLives playerScript = other.GetComponent<PlayerLives>();
            HurtPlayer(playerScript);
        }
    }

    void HurtPlayer(PlayerLives playerScript)
    {
        if (!playerScript) return;
        playerScript.OnTakeDamage();
        Destroy(gameObject);
    }
}
