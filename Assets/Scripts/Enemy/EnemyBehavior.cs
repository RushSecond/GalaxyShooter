using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    protected Enemy _myEnemy;

    [SerializeField]
    protected float _startingZRotation = 0f;
    [SerializeField]
    protected float _mySpeed = 4f;
    [SerializeField]
    private GameObject _myProjectile;
    [SerializeField]
    private Vector3 _projectileOffset;
    [SerializeField]
    private AudioClip _projectileAudio;
    private AudioSource _audioSource;

    protected virtual void Start()
    {
        BehaviorSetup();
    }

    protected void BehaviorSetup()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, _startingZRotation);
        _myEnemy = GetComponent<Enemy>();
        _audioSource = GetComponent<AudioSource>();
    }

    public abstract void Act();

    protected void FireProjectile(Quaternion rotation)
    {
        Instantiate(_myProjectile, transform.position + _projectileOffset, rotation);
        _audioSource.clip = _projectileAudio;
        _audioSource.Play();
    }
}
