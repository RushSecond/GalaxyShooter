using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyInstance
{
    public GameObject enemy;
    public float timeDelay;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnWave", order = 1)]
public class SpawnWave : ScriptableObject
{
    [SerializeField]
    private EnemyInstance[] _waveObjects;

    public EnemyInstance[] WaveObjects { get => _waveObjects; }
}
