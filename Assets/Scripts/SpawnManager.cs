using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private float _durationOfEntireWave;
    private List<GameObject> _aliveEnemies;
    private bool _allWaveEnemiesAreCreated;
    [SerializeField]
    private float _timeBetweenWaves;
    [SerializeField]
    private int _firstWaveNumberEnemies;
    [SerializeField]
    private int _moreEnemiesPerWave;
    private int _waveNumber;

    [SerializeField]
    private GameObject[] _enemyTypes;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject[] _powerups;

    private bool _stopSpawning = false;

    private UIManager _UIManager;

    private void Start()
    {
        _UIManager = FindObjectOfType<UIManager>();
        if (!_UIManager)
            Debug.LogError("UI Manager is null");

        _aliveEnemies = new List<GameObject>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P)) // testing powerups
            ChooseAndCreatePowerup();
#endif
    }

    public void StartSpawning() // called by player when the asteroid is destroyed
    {
        _waveNumber = 1;
        StartCoroutine(WaveSpawnRoutine());
        StartCoroutine(SpawnPowerupRoutine());
    }

    IEnumerator WaveSpawnRoutine()
    {
        yield return new WaitForSeconds(_timeBetweenWaves);
        _UIManager.OnStartNewWave(_waveNumber); // tell the UI so it can display wave number on screen
        _allWaveEnemiesAreCreated = false;
        _aliveEnemies.Clear();
        // get number of enemies
        int numberOfEnemies = _firstWaveNumberEnemies + _moreEnemiesPerWave * (_waveNumber - 1);
        int enemiesCreatedSoFar = 0;
        WaitForSeconds timeBetweenEnemies = new WaitForSeconds(_durationOfEntireWave / (float)numberOfEnemies);

        while (!_stopSpawning && !_allWaveEnemiesAreCreated)
        {
            yield return timeBetweenEnemies;
            // spawn an enemy
            int enemyIndex = Random.Range(0, _enemyTypes.Length);
            GameObject newEnemy = Instantiate(_enemyTypes[enemyIndex], Vector3.zero, Quaternion.Euler(0, 0, -90));
            newEnemy.transform.parent = _enemyContainer.transform;
            newEnemy.GetComponent<Enemy>().SetNewMovementType(Random.Range(0, 2));
            // add enemy to list          
            _aliveEnemies.Add(newEnemy);
            if (++enemiesCreatedSoFar >= numberOfEnemies) _allWaveEnemiesAreCreated = true;
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

    public void OnEnemyDeath(GameObject enemyThatDied)
    {
        _aliveEnemies.Remove(enemyThatDied);
        if (_aliveEnemies.Count == 0 && _allWaveEnemiesAreCreated)
        {
            _waveNumber++;
            StartCoroutine(WaveSpawnRoutine()); // start the next wave
        }
    }

    /// <summary>
    /// Gets a a random point at the right of the screen
    /// </summary>
    public static Vector3 RandomPositionAtRight()
    {
        Bounds cameraBounds = CameraManager.GetCameraBounds();
        float xPosition = cameraBounds.center.x + cameraBounds.extents.x + 1f;

        float yMax = cameraBounds.center.y + cameraBounds.extents.y - 0.5f;
        float yPosition = Random.Range(-yMax, yMax);

        return new Vector3(xPosition, yPosition, 0);
    }
}