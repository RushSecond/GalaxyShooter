using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _mySpeed = 4f;
    [SerializeField]
    private int _scoreValue = 10;

    private UIManager _UIManager;

    // Animator component
    private Animator _myAnimator;

    void Start()
    {
        _UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (!_UIManager)
            Debug.LogError(this + " an enemy couldn't find the UIManager script.");
        
        // Setup and null check
        _myAnimator = GetComponent<Animator>();
        if (!_myAnimator)
            Debug.LogError(this + " an enemy doesn't have an animator.");
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
                playerScript.Damage();
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
        // Enemy should no longer move or collide
        _mySpeed = 0f;
        GetComponent<Collider2D>().enabled = false;

        // Play animation
        _myAnimator.SetTrigger("OnEnemyDeath");

        // Destroy after 5 seconds so the animation can play
        Destroy(gameObject, 2.8f);
    }
}
