using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum MovementType {BasicMovement, Elliptical}

    [SerializeField]
    private MovementType _movementType;
    protected EnemyMovement _myMovement;

    public void SetNewMovementType(int moveType)
    {
        _movementType = (MovementType)moveType;
        SetupMovementType();
    }

    [SerializeField]
    private float _mySpeed = 4f;
    [SerializeField]
    protected GameObject _enemyLaser;
    private float _laserOffsetX = -1f;
    private Coroutine fireRoutine;
    [SerializeField]
    private int _scoreValue = 10;
    [SerializeField]
    private AudioClip _laserAudio;
    [SerializeField]
    private AudioClip _explosionAudio;
    private AudioSource _audioSource;
    [SerializeField]
    private GameObject _explosion;

    public bool _isDead { get; private set; } = false;

    private UIManager _UIManager;
    private SpawnManager _spawnManager;
    private Animator _myAnimator;

    protected void Start()
    {
        SetupMovementType();

        _UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (!_UIManager)
            Debug.LogError(this + " an enemy couldn't find the UIManager script.");

        _spawnManager = FindObjectOfType<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");

        _myAnimator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        fireRoutine = StartCoroutine(FireLaserRoutine());
    }

    protected virtual void SetupMovementType()
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

    protected void Update()
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

        HandleDeathEffects();
    }

    protected virtual IEnumerator FireLaserRoutine()
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

    void HandleDeathEffects()
    {
        // Play animation and sound
        if (_myAnimator)
        {
            _myAnimator.SetTrigger("OnEnemyDeath");
            _audioSource.clip = _explosionAudio;
            _audioSource.Play();
            Destroy(gameObject, 2.8f);
            return;
        }

        // Othewise instatiate an explostion
        GameObject explosion = Instantiate(_explosion, this.transform.position, Quaternion.identity);
        Destroy(explosion, 3f); // Destroy the new explosion after 3 seconds  
        Destroy(this.gameObject, 0.3f); // Detroy us after 0.3 seconds
    }
}
