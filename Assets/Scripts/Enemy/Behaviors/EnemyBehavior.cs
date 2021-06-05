using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    protected Enemy _myEnemy;

    protected float _startingZRotation = 0f;
    [SerializeField]
    protected float _mySpeed = 4f;
    [SerializeField]
    protected GameObject _myProjectile;
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
        _startingZRotation = transform.localEulerAngles.z;
        _myEnemy = GetComponent<Enemy>();
        _audioSource = GetComponent<AudioSource>();
    }

    public abstract void Act();

    protected void SetDefaultRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, _startingZRotation);
    }

    protected void FireProjectile(float Zrotation)
    {
        FireProjectile(Zrotation, _projectileOffset);
    }

    protected void FireProjectile(float Zrotation, Vector3 offset)
    {
        Quaternion rotation = Quaternion.Euler(_myProjectile.transform.eulerAngles + new Vector3(0, 0, Zrotation));
        Instantiate(_myProjectile, transform.position + offset, rotation);
        PlaySound(_projectileAudio);
    }

    protected void PlaySound(AudioClip sound)
    {
        _audioSource.clip = sound;
        _audioSource.Play();
    }
}
