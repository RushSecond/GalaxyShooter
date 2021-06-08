using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour, IProjectile
{
    [SerializeField]
    int _damage = 1;

    public int GetDamage()
    {
        return _damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player
        if (other.gameObject.tag == "Player")
        {
            PlayerLives playerScript = other.GetComponent<PlayerLives>();
            if (!playerScript) return;
            playerScript.OnTakeDamage();
        }
    }
}
