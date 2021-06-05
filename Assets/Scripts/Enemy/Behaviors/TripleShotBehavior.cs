using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleShotBehavior : EnemyBehavior
{
    [SerializeField]
    float _sinusoidAmplitude = 4f;
    [SerializeField]
    float _sinusoidFrequency = 0.25f;
    float _sinusoidCurrentTime;
    [SerializeField]
    float _timeToTurnAround = 0.75f;
    [SerializeField]
    float _tripleShotCooldown = 3f;
    [SerializeField]
    float _shotAngleSpread = 45f;
    [SerializeField]
    Vector3[] _projectileOffsets;

    bool _hasTurnedAround;
    Transform _playerTransform;

    protected override void Start()
    {
        base.Start();
        ResetMovement();
        _playerTransform = FindObjectOfType<Player>().transform;
        StartCoroutine(FireTripleShotRoutine());

        if (_projectileOffsets.Length != 3)
            Debug.LogError(this + " should have exactly 3 projectile offsets");
    }

    void ResetMovement()
    {
        _sinusoidCurrentTime = Random.Range(0f, Mathf.PI * 2);
        transform.position = CameraManager.RandomPositionAtRight();
        SetDefaultRotation();
        _hasTurnedAround = false;
    }

    public override void Act()
    {
        float xPosition = transform.position.x - _mySpeed * Time.deltaTime;

        if (xPosition <= -CameraManager.GetCameraBounds().extents.x - 1f)
        {
            ResetMovement();
            return;
        }

        _sinusoidCurrentTime += Mathf.PI * 2 * _sinusoidFrequency * Time.deltaTime;
        float yPosition = _sinusoidAmplitude * Mathf.Sin(_sinusoidCurrentTime);

        transform.position = new Vector3(xPosition, yPosition, 0f);

        // Check if we are behind the player, and turn around
        if (_playerTransform && xPosition < _playerTransform.position.x && !_hasTurnedAround)
        {
            _hasTurnedAround = true;
            StartCoroutine(TurnAroundRoutine());
        }
    }

    IEnumerator TurnAroundRoutine()
    {
        float elapsedTime = 0f;
        float targetZRotation = _startingZRotation + 180;

        while (elapsedTime < _timeToTurnAround)
        {         
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _timeToTurnAround;
            float currentAngle = Mathf.Lerp(_startingZRotation, targetZRotation, progress);
            transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        } 
    }

    IEnumerator FireTripleShotRoutine()
    {
        WaitForSeconds fireCooldown = new WaitForSeconds(_tripleShotCooldown);
        yield return fireCooldown;

        while (!_myEnemy.enemyLives.IsDead)
        {
            float[] firingAngles = new float[3];
            float rotationFromDefault = transform.localEulerAngles.z - _startingZRotation;
            Quaternion offsetRotation = Quaternion.Euler(0, 0, rotationFromDefault);
            firingAngles[0] = 180 + rotationFromDefault;
            firingAngles[1] = firingAngles[0] + _shotAngleSpread;
            firingAngles[2] = firingAngles[0] - _shotAngleSpread;

            for (int i = 0; i < 3; i++) // fire all 3 lasers
                FireProjectile(firingAngles[i], offsetRotation * _projectileOffsets[i]);

            yield return fireCooldown;
        }
    }
}
