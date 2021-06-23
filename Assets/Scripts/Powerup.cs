using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour, ISpawnChanceWeight
{
    [SerializeField]
    private float _mySpeed = 3f;
    private Transform _magnetizedTransform;
    [SerializeField]
    private float _magnetizedSpeed = 6f;

    public enum PowerupType {
        TripleShot,
        Speed,
        Shield,
        Ammo,
        Repair,
        Missile,
        Upgrade
    }

    [SerializeField]
    private PowerupType _powerupID;  
    public PowerupType getPowerupType()
    {
        return _powerupID;
    }

    [SerializeField]
    private AudioClip _powerupAudio;
    [SerializeField]
    private float _audioVolume = 0.6f;

    [SerializeField]
    private int _spawnChanceWeight = 5;
    public int GetSpawnWeight() => _spawnChanceWeight;
    public int GetWeightIncreasePerWave() => 0;

    private void Start()
    {
        if (_powerupAudio == null)
            Debug.LogError(this + " powerup audio is not assigned.");
    }

    void Update()
    {
        // Move toward the player if they have magnetized this
        if (_magnetizedTransform)
        {
            Vector3 directionToMove = Vector3.Normalize(_magnetizedTransform.position - transform.position);
            transform.position += _magnetizedSpeed * Time.deltaTime * directionToMove;
            return;
        }

        transform.position += Vector3.left * _mySpeed * Time.deltaTime;

        if (transform.position.x <= -CameraManager.GetCameraBounds().extents.x - 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "PlayerPickup")
        {
            Player playerScript = other.GetComponent<Player>();
            if (!playerScript)
                playerScript = other.GetComponentInParent<Player>();
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

        playerScript.OnPickupPowerup(this);

        AudioSource.PlayClipAtPoint(_powerupAudio, Camera.main.transform.position, _audioVolume);
        Destroy(gameObject);
    }

    public void OnMagnetized(Transform player)
    {
        _magnetizedTransform = player;
    }
}