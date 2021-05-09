using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private static float _screenBoundsX = 8.5f;
    [SerializeField]
    private static float _screenBoundsY = 5.8f;

    [SerializeField]
    private float _spawnTime = 5f;
    [SerializeField]
    private GameObject _spawnEnemy;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject[] _powerups;

    private bool _stopSpawning = false;

    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    { 
        WaitForSeconds waitSeconds = new WaitForSeconds(_spawnTime);
        yield return new WaitForSeconds(2f);

        while (!_stopSpawning)
        {
            GameObject newEnemy = Instantiate(_spawnEnemy, RandomPositionAtTop(), Quaternion.identity);
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
            yield return waitRandomSeconds;
            
            int randomIndex = Random.Range(0, 3);
            GameObject newPowerup = Instantiate(_powerups[randomIndex], RandomPositionAtTop(), Quaternion.identity);
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }

    /// <summary>
    /// Gets a a random point at the top of the screen
    /// </summary>
    public static Vector3 RandomPositionAtTop()
    {
        float xPosition = Random.Range(-_screenBoundsX, _screenBoundsX);
        float yPosition = _screenBoundsY;
        return new Vector3(xPosition, yPosition, 0);
    }
}

