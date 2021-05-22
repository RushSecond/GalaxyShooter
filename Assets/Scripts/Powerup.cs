using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 3f;

    private enum PowerupType {TripleShot, Speed, Shield, Ammo, Repair, Missile}
    [SerializeField]
    private PowerupType _powerupID;
    [SerializeField]
    private int _spawnChanceWeight = 5;
    [SerializeField]
    private AudioClip _powerupAudio;
    [SerializeField]
    private float _audioVolume = 0.6f;

    public int GetSpawnWeight { get => _spawnChanceWeight; }

    private void Start()
    {
        if (_powerupAudio == null)
            Debug.LogError(this + " powerup audio is not assigned.");
    }

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
            Debug.LogError(this + " couldn't find the player script");
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
            case 3:
                playerScript.GainAmmo();
                break;
            case 4:
                playerScript.RepairPowerup();
                break;
            case 5:
                playerScript.GainMissiles();
                break;
        }

        AudioSource.PlayClipAtPoint(_powerupAudio, Camera.main.transform.position, _audioVolume);
        Destroy(gameObject);
    }
}

