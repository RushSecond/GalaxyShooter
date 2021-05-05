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

    void Start()
    {
        // Setup the UI manager
        _UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (!_UIManager)
            Debug.LogError(this + " an enemy couldn't find the UIManager script.");
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
            Destroy(gameObject);
        }

        if (other.tag == "Laser")
        {
            // Tell the UIManager to add 10 to the score
            _UIManager.AddScore(_scoreValue);

            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
