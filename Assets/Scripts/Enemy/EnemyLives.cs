using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLives : LivesComponent
{
    [SerializeField]
    private int _scoreValue = 10;
    [SerializeField]
    private AudioClip _explosionAudio;

    private AudioSource _audioSource;
    private Animator _myAnimator;

    protected override void Start()
    {
        base.Start();
        _audioSource = GetComponent<AudioSource>();
        _myAnimator = GetComponent<Animator>();
    }
    // damaging objects when colliding with a laser or player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript)
                playerScript.playerLives.OnTakeDamage();

            OnDeath();
        }

        if (other.tag == "Laser")
        {
            int damage = other.GetComponent<I_Weapon>().GetDamage();
            OnTakeDamage(damage);
            Destroy(other.gameObject);      
        }
    }

    protected override void OnDeath()
    {
        IsDead = true;
        _UIManager.AddScore(_scoreValue);
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