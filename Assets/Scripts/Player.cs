using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 5f;
    [SerializeField]
    private float _boostedSpeed = 8.5f;
    private bool _useBoostSpeed = false;
    [SerializeField]
    private float _speedDuration = 5f;
    private Coroutine _speedRoutine;

    [SerializeField]
    private int _lives = 3;
    private bool _useShields = false;
    [SerializeField]
    private GameObject _shieldObject;

    [SerializeField]
    private float _fireRate = 0.5f;
    private float _cooldownTime = -1f;

    [SerializeField]
    private GameObject _myLaser;
    private bool _useTripleShot = false;
    [SerializeField]
    private GameObject _myTripleLaser;
    [SerializeField]
    private float _tripleShotTime = 5f;
    private Coroutine _tripleShotRoutine;

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
        // Reduce the cooldown time
        _cooldownTime -= Time.deltaTime;

        // If fire button is pressed --AND no cooldown is remaining-- then create a laser, and reset the cooldown
        if (Input.GetButton("Fire1") && _cooldownTime <= 0f)
        {
            FireLaser();
        }

        CalculateMovement();
    }

    void FireLaser()
    {
        //if triple shot is active, fire the triple laser
        if(_useTripleShot)
        {
            Vector3 tripleLaserPosition = transform.position;
            GameObject.Instantiate(_myTripleLaser, tripleLaserPosition, Quaternion.identity);
        }
        //else fire the single laser
        else
        {
            Vector3 laserPosition = transform.position + new Vector3(0, _laserOffsetY, 0);
            GameObject.Instantiate(_myLaser, laserPosition, Quaternion.identity);
        }

        _cooldownTime = _fireRate;
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0f);

        // NEW, get the player's speed based on whether he is boosting or not
        float speed = _useBoostSpeed ? _boostedSpeed : _mySpeed;

        // Move the player by "speed" units, every second
        transform.position += direction * speed * Time.deltaTime;

        // clamp player y
        if (transform.position.y > 0)
        {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
        else if (transform.position.y < -4f)
        {
            transform.position = new Vector3(transform.position.x, -4f, 0);
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
        // if shields are active
        // deactivate shields
        // do nothing else
        if (_useShields)
        {
            ToggleShields(false);
            return;
        }

        _lives--;

        if (_lives <= 0)
        {
            _spawnManager.OnPlayerDeath();

            Destroy(gameObject);
        }
    }

    public void ToggleShields(bool shieldOn)
    {
        _useShields = shieldOn;
        _shieldObject.SetActive(shieldOn);
    }

    public void StartTripleShot()
    {
        if (_tripleShotRoutine != null)
            StopCoroutine(_tripleShotRoutine);
        _useTripleShot = true;
        _tripleShotRoutine = StartCoroutine(EndTripleShot());
    }

    IEnumerator EndTripleShot()
    {
        yield return new WaitForSeconds(_tripleShotTime);
        _useTripleShot = false;
    }

    public void StartSpeed()
    {
        _useBoostSpeed = true;

        if (_speedRoutine != null)
            StopCoroutine(_speedRoutine);

        _speedRoutine = StartCoroutine(EndSpeed());
    }

    IEnumerator EndSpeed()
    {
        yield return new WaitForSeconds(_speedDuration);
        _useBoostSpeed = false;
    }
}