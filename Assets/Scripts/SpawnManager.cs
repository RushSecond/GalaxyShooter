using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private float _spawnTime = 5f;
    [SerializeField]
    private GameObject _spawnEnemy;

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
        while (true)
        {
            Instantiate(_spawnEnemy, Enemy.RandomPositionAtTop(), Quaternion.identity);
            yield return waitSeconds;
        }
    }
}

