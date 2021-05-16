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
    private float _screenWrapY = 5.2f;

    [SerializeField]
    private int _lives = 3;
    private bool _useShields = false;
    [SerializeField]
    private GameObject _shieldObject;
    [SerializeField]
    private GameObject[] _damageEffectObjects;
    [SerializeField]
    private GameObject _explosion;

    [SerializeField]
    private float _fireRate = 0.5f;
    private float _cooldownTime = -1f;
    [SerializeField]
    private GameObject _myLaser;
    private float _laserOffsetX = 0.7f;
    private bool _useTripleShot = false;
    [SerializeField]
    private GameObject _myTripleLaser;
    [SerializeField]
    private float _tripleShotTime = 5f;
    private Coroutine _tripleShotRoutine;


    private SpawnManager _spawnManager;
    private UIManager _UIManager;

    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _laserAudio;

    private void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");

        _UIManager = FindObjectOfType<UIManager>();
        if (!_UIManager)
            Debug.LogError("UI Manager is null");

        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource)
            Debug.LogError("Player audio is null");
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
            GameObject.Instantiate(_myTripleLaser, tripleLaserPosition, transform.rotation);
        }
        //else fire the single laser
        else
        {
            Vector3 laserPosition = transform.position + new Vector3(_laserOffsetX, 0, 0);
            GameObject.Instantiate(_myLaser, laserPosition, transform.rotation);
        }

        _cooldownTime = _fireRate;

        _audioSource.clip = _laserAudio;
        _audioSource.Play();
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = Vector3.Normalize(new Vector3(horizontalInput, verticalInput, 0f));

        // NEW, get the player's speed based on whether he is boosting or not
        float speed = _useBoostSpeed ? _boostedSpeed : _mySpeed;

        // Move the player by "speed" units, every second
        transform.position += direction * speed * Time.deltaTime;

        // clamp player x
        float xPosition = Mathf.Clamp(transform.position.x, -7.5f, 0f);
        
        // screen wrap on y.
        float yPosition = transform.position.y;
        if (Mathf.Abs(yPosition) > _screenWrapY)
        {
            yPosition = -_screenWrapY * Mathf.Sign(yPosition);
        }

        transform.position = new Vector3(xPosition, yPosition, 0);
    }

    public void OnTakeDamage()
    {
        if (_useShields)
        {
            ToggleShields(false);
            return;
        }

        _lives--;
        _UIManager.UpdateLives(_lives);

        switch(_lives) // Effects on taking damage
        {
            case 2:
                _damageEffectObjects[0].SetActive(true);
                break;
            case 1:
                _damageEffectObjects[1].SetActive(true);
                break;
            case 0:
                PlayerDeath();
                break;
        }
    }

    void PlayerDeath()
    {
        _spawnManager.OnPlayerDeath();

        // Stop movement
        _mySpeed = 0f;
        _boostedSpeed = 0f;

        //Create explosion and destroy player
        Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(gameObject, 0.3f);
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