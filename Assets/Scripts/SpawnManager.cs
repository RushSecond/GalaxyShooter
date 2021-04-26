using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private float _spawnTime = 5f;
    [SerializeField]
    private GameObject _spawnEnemy;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // spawn game objects every 5 seconds
    // create a coroutine of type IEnumerator
    // with a while loop

    IEnumerator SpawnRoutine()
    {
        // while loop (infinite loop)
        // Instantiate enemy prefab
        // yield wait for 5 seconds

        WaitForSeconds waitSeconds = new WaitForSeconds(_spawnTime);

        while (true)
        {
            Instantiate(_spawnEnemy, Enemy.RandomPositionAtTop(), Quaternion.identity);
            yield return waitSeconds;
        }
    }
}
