using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Speed and Bounds")]
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
    private bool _speedPowerupActive = false;
    [SerializeField]
    private float _speedDuration = 5f;
    private Coroutine _speedRoutine;
    [SerializeField]
    private float _screenWrapY = 5.2f;

    [Header("Thrusters")]
    [SerializeField]
    private float _heatGainedPerSecond = 0.25f;
    [SerializeField]
    private float _heatLostPerSecond = 0.125f;
    private float _heatAmount = 0f;
    private bool _overheating = false; 

    [Header("Lives, Shields, Damage")]   
    [SerializeField]
    private int _maxLives = 3;
    private int _lives = 0;
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

    [Header("Lasers")]
    [SerializeField]
    private float _fireRate = 0.5f;
    private float _laserCooldownTime = -1f;
    [SerializeField]
    private GameObject _myLaser;
    private float _laserOffsetX = 0.7f;
    [SerializeField]
    private int _ammoMaximum = 15;
    private int _ammoCurrent;

    [Header("TripleShot")] 
    [SerializeField]
    private GameObject _myTripleLaser;
    private bool _useTripleShot = false;
    [SerializeField]
    private float _tripleShotTime = 5f;
    private Coroutine _tripleShotRoutine;

    [Header("Missiles")]
    [SerializeField]
    private GameObject _myMissile;
    [SerializeField]
    private Vector3 _missileOffset;
    [SerializeField]
    private float _missileCooldownTime = 1f;
    [SerializeField]
    private int _maxMissiles = 5;
    private int _missileCount;
    private bool _canShootMissile = true;

    private SpawnManager _spawnManager;
    private UIManager _UIManager;

    [Header("Sound")]
    [SerializeField]
    private AudioClip _laserAudio;
    private AudioSource _audioSource;

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
        GainMissiles();
        GainLives(_maxLives);
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

        // If fire2 button is pressed, no missile cooldown is remaining, and there's ammo,
        // then fire a missile
        if (Input.GetButton("Fire2") && _canShootMissile && _missileCount > 0)
        {
            FireMissile();
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

    void FireMissile() // shoot two missiles
    {
        // Mirror on Y
        Vector3 _mirrorOffset = Vector3.Scale(_missileOffset, new Vector3(1, -1, 1));

        Vector3 missilePosition1 = transform.position + _missileOffset;     
        Vector3 missilePosition2 = transform.position + _mirrorOffset;

        GameObject.Instantiate(_myMissile, missilePosition1, transform.rotation);
        GameObject.Instantiate(_myMissile, missilePosition2, transform.rotation);

        _missileCount--; // update number of missiles and tell the UI
        _UIManager.UpdateMissileCount(_missileCount);

        _canShootMissile = false; // cooldown
        StartCoroutine(MissileCooldown());
    }

    IEnumerator MissileCooldown()
    {
        yield return new WaitForSeconds(_missileCooldownTime);
        _canShootMissile = true;
    }

    public void GainMissiles()
    {
        _missileCount = _maxMissiles;
        _UIManager.UpdateMissileCount(_missileCount);
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

        GainLives(-1);
    }

    void GainLives(int livesGained)
    {
        // new lives should be clamped between 0 and max
        int newLifeAmount = Mathf.Clamp(_lives + livesGained, 0, _maxLives);
        _UIManager.UpdateLives(newLifeAmount);

        if (newLifeAmount == 0)
        {
            PlayerDeath();
            return;
        }

        if (livesGained > 0) // gained life, so turn off some damage effects
        {
            // if lives go from 1 to 3, we want to turn off
            // effects 0 - 2 , so we subtract each by one
            ToggleDamageEffects(_lives - 1, newLifeAmount - 1, false);
        }

        if (livesGained < 0) // lost life, so turn on some damage effects
        {
            ToggleDamageEffects(newLifeAmount - 1, _lives - 1, true);
        }

        _lives = newLifeAmount; // do this last, so we can use _lives
        // to properly set the damage effects
    }

    /// <summary>
    /// Toggles the player's damage effect objects from startIndex(inclusive) to endIndex(exclusive)
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <param name="turnOn"></param>
    void ToggleDamageEffects(int startIndex, int endIndex, bool turnOn)
    {
        for (int index = startIndex; index < endIndex; index++)
        {          
            if (index >= 0 && index < _damageEffectObjects.Length // prevent out of bounds error
                && _damageEffectObjects[index] != null) // if damage effect is missing, just skip it
            {
                _damageEffectObjects[index].SetActive(turnOn);
            }
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
        _lives = 0;
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

    public void RepairPowerup()
    {
        GainLives(1);
    }
}