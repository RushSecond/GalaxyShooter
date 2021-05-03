using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 3f;


    //ID for powerups
    // 0 = Triple Shot
    // 1 = Speed
    // 2 = Shields
    private enum PowerupType {TripleShot, Speed, Shield}
    [SerializeField]
    private PowerupType _powerupID;

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
            GrantPowerup(playerScript);
            Destroy(gameObject);
        }
    }

    private void GrantPowerup(Player playerScript)
    {
        if (!playerScript)
            return;

        switch ((int)_powerupID)
        {
            case 0:
                playerScript.StartTripleShot();
                break;
            case 1:
                playerScript.StartSpeed();
                break;
            case 2:
                playerScript.ToggleShields(true);
                break;
        }
    }

}

