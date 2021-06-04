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
    private float _shieldedEnemyChancePerWave = 0.1f;
    private float _shieldedEnemyChance = 0f;

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
            Instantiate(ChooseWeightedItem(_powerups), CameraManager.RandomPositionAtRight(), Quaternion.identity);
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
            GameObject newEnemy = ChooseWeightedItem(_enemyTypes);
            newEnemy = Instantiate(newEnemy, Vector3.zero, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            // add enemy to list          
            _aliveEnemies.Add(newEnemy);
            if (++enemiesCreatedSoFar >= numberOfEnemies) _allWaveEnemiesAreCreated = true;
            // chance to add shields to enemies
            float shieldRandom = Random.Range(0f, 1f);
            if (shieldRandom < _shieldedEnemyChance)
            {
                EnemyLives lifeComponent = newEnemy.GetComponent<EnemyLives>();
                lifeComponent.ToggleShields(true);
            }
        }
    }

    IEnumerator SpawnPowerupRoutine()
    {
        // every 3-7 seconds, spawn a powerup
        while (!_stopSpawning)
        {
            yield return new WaitForSeconds(Random.Range(3, 8));
            GameObject chosenPowerup = ChooseWeightedItem(_powerups);
            Instantiate(chosenPowerup, CameraManager.RandomPositionAtRight(), Quaternion.identity);
        }
    }

    GameObject ChooseWeightedItem(GameObject[] objects)
    {
        int totalWeight = 0;
        int[] weights = new int[objects.Length];
        for (int i = 0; i < objects.Length; i++) 
        {
            weights[i] = objects[i].GetComponent<ISpawnChanceWeight>().GetSpawnWeight();
            totalWeight += weights[i]; // get a total of all weights in the list
        }

        int random = Random.Range(0, totalWeight);
        for (int i = 0; i < objects.Length; i++)
        {
            if(random < weights[i]) // more likely to be true for larger weights
            {
                return objects[i];
            }
            // guarantees random will be less than some weight
            random -= weights[i];
        }
        // we should never reach this point
        Debug.LogError(this + "Something has gone wrong with ChooseWeightedItem");
        return null;
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
            _shieldedEnemyChance += _shieldedEnemyChancePerWave; // Add to shielded enemy chance
            StartCoroutine(WaveSpawnRoutine()); // start the next wave
        }
    }
}