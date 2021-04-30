using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 1f;
    [SerializeField]
    private int _lives = 3;

    [SerializeField]
    private float _fireRate = 0.5f;
    private float _cooldownTime = -1f;

    [SerializeField]
    private GameObject _myLaser;

    private SpawnManager _spawnManager;

    private float _laserOffsetY = 0.7f;

    private void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();

        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");
    }

    void Update()
    {
        CalculateMovement();
        TryFireLaser();
    }

    void TryFireLaser()
    {
        // Reduce the cooldown time
        _cooldownTime -= Time.deltaTime;

        // If fire button is pressed --AND no cooldown is remaining-- then create a laser, and reset the cooldown
        if (Input.GetButton("Fire1") && _cooldownTime <= 0f)
        {
            Vector3 laserPosition = transform.position + new Vector3(0, _laserOffsetY, 0);
            GameObject.Instantiate(_myLaser, laserPosition, Quaternion.identity);
            _cooldownTime = _fireRate;
        }
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);

        // Move the player by "mySpeed" units, every second
        transform.position += direction * _mySpeed * Time.deltaTime;

        // clamp player y
        if (transform.position.y > 0)
        {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
        else if (transform.position.y < -4.5f)
        {
            transform.position = new Vector3(transform.position.x, -4.5f, 0);
        }

        // screen wrap on x.
        float xPosition = transform.position.x;
        if (Mathf.Abs(xPosition) > 9.4f)
        {
            transform.position = new Vector3(-9.4f * Mathf.Sign(xPosition), transform.position.y, 0);
        }
    }

    public void Damage()
    {
        _lives--;

        if (_lives <= 0)
        {
            // Communicate with spawn manager
            // Let them know player is dead
            _spawnManager.OnPlayerDeath();

            Destroy(gameObject);
        }
    }
}