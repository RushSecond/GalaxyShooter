using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _baseSpeed = 3f;
    [SerializeField]
    private float _thrusterSpeed = 6f;
    [SerializeField]
    private Transform _thrusterTransform;
    [SerializeField]
    private float _thrusterSizeMultiplier = 1.8f;
    [SerializeField]
    private float _speedPowerupMutliplier = 1.6f;


    private float _heatAmount = 0f;
    private bool _overheating = false;
    [SerializeField]
    private float _heatGainedPerSecond = 0.25f;
    [SerializeField]
    private float _heatLostPerSecond = 0.125f;

    private bool _speedPowerupActive = false;
    [SerializeField]
    private float _speedDuration = 5f;
    private Coroutine _speedRoutine;
    [SerializeField]
    private float _screenWrapY = 5.2f;

    [SerializeField]
    private int _lives = 3;
    private int _shieldHP = 0;
    [SerializeField]
    private int _shieldMaxHP = 3;
    [SerializeField]
    private GameObject _shieldObject;
    [SerializeField]
    private Color _shieldColorOriginal;

    [SerializeField]
    private GameObject[] _damageEffectObjects;
    [SerializeField]
    private GameObject _explosion;

    [SerializeField]
    private float _fireRate = 0.5f;
    private float _laserCooldownTime = -1f;
    [SerializeField]
    private GameObject _myLaser;
    private float _laserOffsetX = 0.7f;
    [SerializeField]
    private int _ammoMaximum = 15;
    private int _ammoCurrent;

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

        GainAmmo();
    }

    void Update()
    {
        // Reduce the cooldown time
        _laserCooldownTime -= Time.deltaTime;

        // If fire button is pressed, no cooldown is remaining, and there's ammo,
        // then fire a laser
        if (Input.GetButton("Fire1") && _laserCooldownTime <= 0f && _ammoCurrent > 0)
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
        else //else fire the single laser
        {
            Vector3 laserPosition = transform.position + new Vector3(_laserOffsetX, 0, 0);
            GameObject.Instantiate(_myLaser, laserPosition, transform.rotation);
        }

        _laserCooldownTime = _fireRate; // reset fire cooldown
        _ammoCurrent--; // reduce ammo and tell the UI Manager
        _UIManager.UpdateAmmoCount(_ammoCurrent);

        _audioSource.clip = _laserAudio;
        _audioSource.Play();
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = Vector3.Normalize(new Vector3(horizontalInput, verticalInput, 0f));

        // NEW, get the player's speed based on whether he is thrusting and moving
        bool isMoving = direction.magnitude >= .01f;
        float speed = CheckThrusters(isMoving);

        // Multiply player speed if powerup is active
        if (_speedPowerupActive) speed *= _speedPowerupMutliplier;

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

    float CheckThrusters(bool isMoving)
    {
        float speed;
        if (Input.GetAxis("Thrusters") == 1 && !_overheating && isMoving)
        { // Increase speed and heat, make thruster image larger
            speed = _thrusterSpeed;
            _heatAmount += _heatGainedPerSecond * Time.deltaTime;
            _thrusterTransform.localScale = Vector3.one * _thrusterSizeMultiplier;
            if (_heatAmount > 1f) _overheating = true;
        }
        else
        {
            speed = _baseSpeed;
            _heatAmount -= _heatLostPerSecond * Time.deltaTime;
            _thrusterTransform.localScale = Vector3.one;
            if (_heatAmount < 0f) _overheating = false;
        }

        _heatAmount = Mathf.Clamp(_heatAmount, 0f, 1f); // Make sure heat stays between 0 and 1
        _UIManager.UpdateHeatGauge(_heatAmount);

        return speed;
    }

    public void OnTakeDamage()
    {
        if (_shieldHP > 0) // Shields take damage
        {
            ShieldDamage();
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

    void ShieldDamage()
    {
        _shieldHP--;
        if (_shieldHP == 0)
        {
            ToggleShields(false);
            return;
        }

        // Change the color of the shield object to be less bright 
        float fractionOfShieldRemaining = (float)_shieldHP / (float)_shieldMaxHP;
        Color newShieldColor = Color.Lerp(Color.black, _shieldColorOriginal, fractionOfShieldRemaining);
        _shieldObject.GetComponent<SpriteRenderer>().color = newShieldColor;
    }

    void PlayerDeath()
    {
        _spawnManager.OnPlayerDeath();

        // Stop movement
        _baseSpeed = 0f;
        _thrusterSpeed = 0f;

        //Create explosion and destroy player
        Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(gameObject, 0.3f);
    }

    public void ToggleShields(bool shieldOn)
    {  
        _shieldObject.SetActive(shieldOn);
        if (shieldOn)
        {
            _shieldHP = _shieldMaxHP;
            _shieldObject.GetComponent<SpriteRenderer>().color = _shieldColorOriginal;
        }
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
        _speedPowerupActive = true;

        if (_speedRoutine != null)
            StopCoroutine(_speedRoutine);

        _speedRoutine = StartCoroutine(EndSpeed());
    }

    IEnumerator EndSpeed()
    {
        yield return new WaitForSeconds(_speedDuration);
        _speedPowerupActive = false;
    }

    public void GainAmmo()
    {
        _ammoCurrent = _ammoMaximum;
        _UIManager.UpdateAmmoCount(_ammoCurrent);
    }
}