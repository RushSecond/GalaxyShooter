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
    protected AudioClip _projectileAudio;
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

    protected void FireProjectile(Vector3 targetDirection, Vector3 offset)
    {
        float angleToFire = Vector3.SignedAngle(Vector3.right, targetDirection, Vector3.forward);
        FireProjectile(angleToFire, offset);
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
        _audioSource.pitch = 1;
        _audioSource.Play();
    }

    protected void PlaySound(AudioClip sound, float pitch)
    {
        _audioSource.clip = sound;
        _audioSource.pitch = pitch;
        _audioSource.Play();
    }

    protected static void RotateToVector(GameObject obj, Vector3 targetDirection, float defaultZ, float maxRotationSpeed)
    {
        float myCurrentAngle = obj.transform.rotation.eulerAngles.z - defaultZ;
        Vector3 myCurrentFacing = Quaternion.Euler(0, 0, myCurrentAngle) * Vector3.left;
        float angleToRotate = Vector3.SignedAngle(myCurrentFacing, targetDirection, Vector3.forward);
        float maxAngleSpeed = maxRotationSpeed * Time.deltaTime;
        angleToRotate = Mathf.Clamp(angleToRotate, -maxAngleSpeed, maxAngleSpeed);
        obj.transform.rotation *= Quaternion.AngleAxis(angleToRotate, Vector3.forward);
    }

    protected static void RotateToVector(GameObject obj, Vector3 targetDirection, float defaultZ)
    {
        float myCurrentAngle = obj.transform.rotation.eulerAngles.z - defaultZ;
        Vector3 myCurrentFacing = Quaternion.Euler(0, 0, myCurrentAngle) * Vector3.left;
        float angleToRotate = Vector3.SignedAngle(myCurrentFacing, targetDirection, Vector3.forward);
        obj.transform.rotation *= Quaternion.AngleAxis(angleToRotate, Vector3.forward);
    }
}
