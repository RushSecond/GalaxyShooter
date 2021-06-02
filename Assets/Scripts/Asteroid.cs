using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField]
    private float _rotationSpeed = 30f;

    [SerializeField]
    private GameObject _explosion;

    private SpawnManager _spawnManager;

    private void Start()
    {
        _spawnManager = GameObject.FindObjectOfType<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError(this + " can't find the spawn manager");
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Projectile")
        {
            _spawnManager.StartSpawning(); // tell spawn manager to begin

            Destroy(other.gameObject); // Destroy laser
            GameObject explosion = Instantiate(_explosion, this.transform.position, Quaternion.identity);
            Destroy(explosion, 3f); // Destroy the new explosion after 3 seconds  
            GetComponent<Collider2D>().enabled = false; // Turn off our collider
            Destroy(this.gameObject, 0.3f); // Detroy us after 0.3 seconds
        }
    }
}
