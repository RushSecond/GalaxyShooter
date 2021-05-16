using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 3f;

    private enum PowerupType {TripleShot, Speed, Shield}
    [SerializeField]
    private PowerupType _powerupID;
    [SerializeField]
    private AudioClip _powerupAudio;
    [SerializeField]
    private float _audioVolume = 0.6f;

    void Update()
    {
        transform.Translate(Vector3.left * _mySpeed * Time.deltaTime);

        if (transform.position.x <= -SpawnManager._screenBoundsX)
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
        }
    }

    private void GrantPowerup(Player playerScript)
    {
        if (!playerScript)
        {
            Debug.LogError(this + " couldn't find the player script on something tagged as player");
            return;
        }

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

        AudioSource.PlayClipAtPoint(_powerupAudio, Camera.main.transform.position, _audioVolume);
        Destroy(gameObject);
    }
}

