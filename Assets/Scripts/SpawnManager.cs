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

    private bool _stopSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    { 
        WaitForSeconds waitSeconds = new WaitForSeconds(_spawnTime);

        // while loop (infinite loop)
        // Instantiate enemy prefab
        // yield wait for 5 seconds
        while (!_stopSpawning)
        {
            GameObject newEnemy = Instantiate(_spawnEnemy, Enemy.RandomPositionAtTop(), Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return waitSeconds;
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}

