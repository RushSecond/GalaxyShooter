using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum MovementType {BasicMovement, Elliptical}

    [SerializeField]
    private MovementType _movementType;
    private EnemyMovement _myMovement;

    public void SetNewMovementType(int moveType)
    {
        _movementType = (MovementType)moveType;
        SetupMovementType();
    }

    [SerializeField]
    private float _mySpeed = 4f;
    [SerializeField]
    private GameObject _enemyLaser;
    private float _laserOffsetX = -1f;
    private Coroutine fireRoutine;
    [SerializeField]
    private int _scoreValue = 10;
    [SerializeField]
    private AudioClip _laserAudio;
    [SerializeField]
    private AudioClip _explosionAudio;
    private AudioSource _audioSource;

    public bool _isDead { get; private set; } = false;

    private UIManager _UIManager;
    private SpawnManager _spawnManager;
    private Animator _myAnimator;

    void Start()
    {
        SetupMovementType();

        _UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (!_UIManager)
            Debug.LogError(this + " an enemy couldn't find the UIManager script.");

        _spawnManager = FindObjectOfType<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");

        _myAnimator = GetComponent<Animator>();
        if (!_myAnimator)
            Debug.LogError(this + " an enemy doesn't have an animator.");

        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource)
            Debug.LogError(this + " an enemy doesn't have an audio source.");

        fireRoutine = StartCoroutine(FireLaserRoutine());
    }

    void SetupMovementType()
    {
        switch((int)_movementType)
        {
            case 0:
                _myMovement = new EnemyMovement(this);
                break;
            case 1:
                _myMovement = new EllipticalMovement(this);
                break;
        }
    }

    void Update()
    {
        _myMovement.Move(_mySpeed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript)
                playerScript.OnTakeDamage();
            EnemyDeath();
        }

        if (other.tag == "Laser")
        {
            _UIManager.AddScore(_scoreValue);

            Destroy(other.gameObject);
            EnemyDeath();
        }
    }

    private void EnemyDeath()
    {
        _isDead = true;
        _spawnManager.OnEnemyDeath(gameObject); // Tell the spawn manager

        StopCoroutine(fireRoutine);

        // Enemy should no longer move or collide   
        _mySpeed = 0f;
        GetComponent<Collider2D>().enabled = false;
        // Explosion should appear behind other alive enemies
        GetComponent<SpriteRenderer>().sortingOrder = -1; 
        // Play animation and sound
        _audioSource.clip = _explosionAudio;
        _audioSource.Play();
        _myAnimator.SetTrigger("OnEnemyDeath");

        Destroy(gameObject, 2.8f);
    }

    IEnumerator FireLaserRoutine()
    {
        Vector3 laserOffset = new Vector3(_laserOffsetX, 0, 0);
        while (true)
        {
            int fireTime = Random.Range(3, 8);
            yield return new WaitForSeconds(fireTime);

            Instantiate(_enemyLaser, transform.position + laserOffset, transform.rotation);
            _audioSource.clip = _laserAudio;
            _audioSource.Play();
        }
    }
}