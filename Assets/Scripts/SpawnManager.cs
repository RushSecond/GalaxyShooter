using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    public static float _screenBoundsX = 9.7f;
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


    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
            ChooseAndCreatePowerup();
#endif
    }

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
        // every 3-7 seconds, spawn a powerup
        while (!_stopSpawning)
        {
            yield return new WaitForSeconds(Random.Range(3, 8));
            ChooseAndCreatePowerup();
        }
    }

    void ChooseAndCreatePowerup()
    {
        int totalWeight = 0;
        int[] weights = new int[_powerups.Length];
        for (int i = 0; i < _powerups.Length; i++)
        {
            weights[i] = _powerups[i].GetComponent<Powerup>().GetSpawnWeight;
            totalWeight += weights[i];
        }

        int random = Random.Range(0, totalWeight);
        for (int i = 0; i < _powerups.Length; i++)
        {
            if(random < weights[i])
            {
                Instantiate(_powerups[i], RandomPositionAtRight(), Quaternion.identity);
                return;
            }

            random -= weights[i];
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