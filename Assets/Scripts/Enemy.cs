using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 4f;
    [SerializeField]
    private GameObject _enemyLaser;
    [SerializeField]
    private float _laserYOffset = -1f;
    private Coroutine fireRoutine;
    [SerializeField]
    private int _scoreValue = 10;
    [SerializeField]
    private AudioClip _laserAudio;
    [SerializeField]
    private AudioClip _explosionAudio;
    private AudioSource _audioSource;

    private UIManager _UIManager;
    private Animator _myAnimator;

    void Start()
    {
        _UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (!_UIManager)
            Debug.LogError(this + " an enemy couldn't find the UIManager script.");
        
        _myAnimator = GetComponent<Animator>();
        if (!_myAnimator)
            Debug.LogError(this + " an enemy doesn't have an animator.");

        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource)
            Debug.LogError(this + " an enemy doesn't have an audio source.");

        fireRoutine = StartCoroutine(FireLaserRoutine());
    }

    void Update()
    {
        transform.Translate(Vector3.down * _mySpeed * Time.deltaTime);

        // if bottom of screen
        // respawn at top with a new random xPosition
        if (transform.position.y <= -5.8f)
        {
            transform.position = SpawnManager.RandomPositionAtTop();
        }
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
        // Turn off firing
        StopCoroutine(fireRoutine);

        // Enemy should no longer move or collide
        _mySpeed = 0f;
        GetComponent<Collider2D>().enabled = false;

        // Play animation and sound
        _audioSource.clip = _explosionAudio;
        _audioSource.Play();
        _myAnimator.SetTrigger("OnEnemyDeath");

        // Destroy after 5 seconds so the animation can play
        Destroy(gameObject, 2.8f);
    }

    IEnumerator FireLaserRoutine()
    {
        Vector3 laserOffset = new Vector3(0, _laserYOffset, 0);
        while (true)
        {
            int fireTime = Random.Range(3, 8);
            yield return new WaitForSeconds(fireTime);

            Instantiate(_enemyLaser, transform.position + laserOffset, Quaternion.identity);
            _audioSource.clip = _laserAudio;
            _audioSource.Play();
        }
    }
}
