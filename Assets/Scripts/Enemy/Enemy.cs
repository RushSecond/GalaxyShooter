using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyBehavior _myBehavior;

    [SerializeField]
    private int _scoreValue = 10;
    [SerializeField]
    private AudioClip _explosionAudio;
    private AudioSource _audioSource;
    [SerializeField]
    private GameObject _explosion;

    public bool _isDead { get; private set; } = false;

    private UIManager _UIManager;
    private SpawnManager _spawnManager;
    private Animator _myAnimator;

    void Start()
    {
        _myBehavior = GetComponent<EnemyBehavior>();
        if (!_myBehavior)
            Debug.LogError(this + " an enemy doesn't have a behavior script.");

        _UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (!_UIManager)
            Debug.LogError(this + " an enemy couldn't find the UIManager script.");

        _spawnManager = FindObjectOfType<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");

        _myAnimator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!_isDead) _myBehavior.Act();
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

        // Enemy should no longer collide   
        GetComponent<Collider2D>().enabled = false;
        // Explosion should appear behind other alive enemies
        GetComponent<SpriteRenderer>().sortingOrder = -10;

        HandleDeathEffects();
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
