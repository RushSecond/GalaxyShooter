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
    private Bounds _playerBounds;
    private Animator _myAnimator;

    [Header("Thrusters")]
    [SerializeField]
    private float _heatGainedPerSecond = 0.25f;
    [SerializeField]
    private float _heatLostPerSecond = 0.125f;
    private float _heatAmount = 0f;
    private bool _overheating = false; 

    [Header("Lasers")]
    [SerializeField]
    private float _baseFireRate = 0.5f;
    [SerializeField]
    private float _bonusAttackSpeedPerUpgrade = .3f;
    private float _currentAttackSpeed = 1;
    private float _laserCooldownTime = -1f;
    private bool _isUpgradingWeapon = false;
    [SerializeField]
    private GameObject _myLaser;
    private float _laserOffsetX = 0.7f;
    [SerializeField]
    private int _ammoMaximum = 15;
    [SerializeField]
    private int _bonusMaxAmmoPerUpgrade = 10;
    private int _ammoCurrent;
    [SerializeField]
    private float _upgradeTime = 5f;

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

    [Header("Magnet")]
    [SerializeField]
    private float _magnetCooldownTime = 10f;
    private bool _canUseMagnet = true;
    private int _numPowerupsCollected = 0;

    private UIManager _UIManager;
    public PlayerLives playerLives { get; private set; }

    [Header("Sound")]
    [SerializeField]
    private AudioClip _laserAudio;
    private AudioSource _audioSource;
    private GameManager _gameManager;

    private void Start()
    {
        playerLives = GetComponent<PlayerLives>();
        if (!playerLives)
            Debug.LogError("Player doesn't have a lives script.");

        _UIManager = FindObjectOfType<UIManager>();
        if (!_UIManager)
            Debug.LogError("UI Manager is null");

        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource)
            Debug.LogError("Player audio is null");

        _gameManager = FindObjectOfType<GameManager>();
        if (!_gameManager)
            Debug.LogError("Game manager is null");

        _myAnimator = GetComponent<Animator>();
        if (!_myAnimator)
            Debug.LogError("Player doesn't have an animator.");

        GainAmmo();
        GainMissiles();
    }

    void Update()
    {
        if (playerLives.IsDead) return;
        if (_gameManager.IsPaused) return;

        // Reduce the cooldown time
        _laserCooldownTime -= Time.deltaTime;

        // If fire button is pressed, no cooldown is remaining, and there's ammo,
        // then fire a laser
        if (_laserCooldownTime <= 0f && _ammoCurrent > 0
            && !_isUpgradingWeapon && Input.GetButton("Fire1"))
        {
            FireLaser();
        }

        // If fire2 button is pressed, no missile cooldown is remaining, and there's ammo,
        // then fire a missile
        if (Input.GetButton("Fire2") && _canShootMissile && _missileCount > 0)
        {
            FireMissile();
        }

        // If magnet button is pressed, no magnet cooldown is remaining,
        // then activate powerup magnet
        if (Input.GetButton("Magnet") && _canUseMagnet)
        {
            StartCoroutine(PowerupMagnetRoutine());
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

        _laserCooldownTime = _baseFireRate / _currentAttackSpeed; // reset fire cooldown

        _ammoCurrent--; // reduce ammo and tell the UI Manager
        _UIManager.UpdateAmmoCount(_ammoCurrent, _ammoMaximum);

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

    IEnumerator PowerupMagnetRoutine()
    {
        GameObject[] allPowerups = GameObject.FindGameObjectsWithTag("Powerup");
        int numPowerupsToCollect = allPowerups.Length;
        // don't actually use magnet if there's no powerup on screen
        if (numPowerupsToCollect == 0) yield break;

        _numPowerupsCollected = 0;
        _canUseMagnet = false;
        _UIManager.OnMagnetUsed(); // tell UI we are using the magnet
        foreach (GameObject powerup in allPowerups)
        { // Magnetize them all, so they move toward the player
            Powerup powerupScript = powerup.GetComponent<Powerup>();
            powerupScript.OnMagnetized(this.transform);
        }
        while(_numPowerupsCollected < numPowerupsToCollect)
        {
            yield return null; // wait until all powerups are collected
        }
        StartCoroutine(MagnetCooldownRoutine());
    }

    IEnumerator MagnetCooldownRoutine()
    {
        float elapsedTime = 0f;
        _UIManager.UpdateMagnetRecharge(0f); // tell UI to empty gauge
        while (elapsedTime < _magnetCooldownTime)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _magnetCooldownTime;
            // tell UI to recharge gauge
            _UIManager.UpdateMagnetRecharge(progress);      
        }
        _canUseMagnet = true; // cooldown done!
    }

    public void OnPickupPowerup(Powerup powerup)
    {
        _numPowerupsCollected++;

        switch ((int)powerup.getPowerupType())
        {
            case 0:
                StartTripleShot();
                break;
            case 1:
                StartSpeed();
                break;
                // etc
            case 2:
                playerLives.ToggleShields(true);
                break;
            case 3:
                GainAmmo();
                break;
            case 4:
                playerLives.RepairPowerup();
                break;
            case 5:
                GainMissiles();
                break;
            case 6:
                UpgradeWeapon();
                break;
        }
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        _myAnimator.SetFloat("Turning", verticalInput);

        Vector3 direction = Vector3.Normalize(new Vector3(horizontalInput, verticalInput, 0f));

        // NEW, get the player's speed based on whether he is thrusting and moving
        bool isMoving = direction.magnitude >= .01f;
        float speed = CheckThrusters(isMoving);

        // Multiply player speed if powerup is active
        if (_speedPowerupActive) speed *= _speedPowerupMutliplier;

        // Move the player by "speed" units, every second
        transform.position += direction * speed * Time.deltaTime;

        // clamp player x and y in bounds
        float xPosition = Mathf.Clamp(transform.position.x, _playerBounds.min.x, _playerBounds.max.x);
        float yPosition = Mathf.Clamp(transform.position.y, _playerBounds.min.y, _playerBounds.max.y);
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
        _UIManager.UpdateAmmoCount(_ammoCurrent, _ammoMaximum);
    }

    public void UpgradeWeapon()
    {
        if (!_isUpgradingWeapon)
            StartCoroutine(UpgradeRoutine());
    }

    IEnumerator UpgradeRoutine()
    {
        _isUpgradingWeapon = true; // Player can't fire
        float elapsedTime = 0;
        while (elapsedTime < _upgradeTime) // lasts some duration
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            // tell the UI manager to update its upgrade gauge
            _UIManager.UpdateWeaponUpgrade(elapsedTime / _upgradeTime);
        }
        _isUpgradingWeapon = false; // Player can fire aagain!
        // and gets bonuses to the laser weapon!
        _currentAttackSpeed += _bonusAttackSpeedPerUpgrade;
        _ammoMaximum += _bonusMaxAmmoPerUpgrade;
        GainAmmo();
    }
}