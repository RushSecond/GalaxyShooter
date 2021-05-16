using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    public static float _screenBoundsX = 9f;
    [SerializeField]
    public static float _screenBoundsY = 4.5f;

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
            GameObject newEnemy = Instantiate(_spawnEnemy, RandomPositionAtRight(), Quaternion.Euler(0, 0, -90));
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
            GameObject newPowerup = Instantiate(_powerups[randomIndex], RandomPositionAtRight(), Quaternion.identity);
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }

    /// <summary>
    /// Gets a a random point at the right of the screen
    /// </summary>
    public static Vector3 RandomPositionAtRight()
    {
        float xPosition = _screenBoundsX;
        float yPosition = Random.Range(-_screenBoundsY, _screenBoundsY);
        return new Vector3(xPosition, yPosition, 0);
    }
}

