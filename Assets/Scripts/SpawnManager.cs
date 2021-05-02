using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private float _spawnTime = 5f;
    [SerializeField]
    private GameObject _spawnEnemy;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject _tripleShotPowerup;

    private bool _stopSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    { 
        WaitForSeconds waitSeconds = new WaitForSeconds(_spawnTime);

        while (!_stopSpawning)
        {
            GameObject newEnemy = Instantiate(_spawnEnemy, Enemy.RandomPositionAtTop(), Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return waitSeconds;
        }
    }

    IEnumerator SpawnPowerupRoutine()
    {
        // every 3-7 seconds, spawn in a powerup
        while (!_stopSpawning)
        {
            WaitForSeconds waitRandomSeconds = new WaitForSeconds(Random.Range(3, 8));
            GameObject newPowerup = Instantiate(_tripleShotPowerup, Enemy.RandomPositionAtTop(), Quaternion.identity);
            yield return waitRandomSeconds;
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}

