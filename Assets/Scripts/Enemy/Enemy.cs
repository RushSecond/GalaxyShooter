using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, ISpawnChanceWeight
{
    private EnemyBehavior _myBehavior;
    public EnemyLives enemyLives { get; private set; }

    [SerializeField]
    private int _spawnChanceWeight = 5;
    public int GetSpawnWeight() => _spawnChanceWeight;
    [SerializeField]
    private int _spawnWeightPerWave = 0;
    public int GetWeightIncreasePerWave() => _spawnWeightPerWave; 

    void Start()
    {
        enemyLives = GetComponent<EnemyLives>();

        _myBehavior = GetComponent<EnemyBehavior>();
        if (!_myBehavior)
            Debug.LogError(this + " an enemy doesn't have a behavior script.");
    }

    void Update()
    {
        if (!enemyLives.IsDead) _myBehavior.Act();
    }

   
}
