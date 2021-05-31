using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyBehavior _myBehavior;
    public EnemyLives enemyLives { get; private set; }

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
