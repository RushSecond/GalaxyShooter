using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgingBehavior : EnemyBehavior
{
    enum State { Moving, Charging, Firing }
    State _myState;
    float _elapsedTime;

    [SerializeField]
    Bounds _movableArea;
    [SerializeField]
    private float _dodgingAmount = 0.2f;
    [SerializeField]
    private float _laserDetectDistance = 5f;
    [SerializeField]
    private float _dodgeCooldownTime = 3f;
    [SerializeField]
    float _firingCooldown = 2.5f;
    Vector3 _oldPosition;
    Vector3 _moveDestination;
    Collider2D _myCollider;
    float _timeToMove;

    Quaternion _targetRotation;
    [SerializeField]
    float _maxRotationSpeed;
    Transform _playerTransform;

    [SerializeField]
    GameObject _chargeBall;
    Vector3 _maxChargeSize;
    [SerializeField]
    float _timeToCharge;
    [SerializeField]
    GameObject _warningLine;
    [SerializeField]
    float _warningFlashTime;

    [SerializeField]
    float _beamDuration;
    [SerializeField]
    float _beamScalingTime;
    Vector3 _maxBeamSize;
    
    protected override void Start()
    {
        base.Start();
        _playerTransform = FindObjectOfType<Player>().transform;
        transform.position = CameraManager.RandomPositionAtRight();
        _myCollider = GetComponent<Collider2D>();
    
        _maxChargeSize = _chargeBall.transform.localScale;
        _chargeBall.SetActive(false);
        _warningLine.SetActive(false);
        _maxBeamSize = _myProjectile.transform.localScale;
        _myProjectile.SetActive(false);

        ChangeState(State.Moving);
    }

    void ChangeState(State _newState)
    {
        _myState = _newState;
        _elapsedTime = 0;
        switch ((int)_myState)
        {
            case 0:
                SetupMove();
                break;
            case 1:
                SetupCharge();
                break;
            case 2:
                SetupFire();
                break;
        }
    }

    public override void Act()
    {
        switch ((int)_myState)
        {
            case 0:
                DoMove();
                break;
            case 1:
                DoCharge();
                break;
            case 2:
                DoFire();
                break;
        }
    }

    void SetupMove()
    {
        GetNewDestination(null);
        _myProjectile.SetActive(false);
    }

    private void DoMove()
    {
        _elapsedTime += Time.deltaTime;
        float progress = _elapsedTime / _timeToMove;
        // use function 1-(x-1)^2 so we can suddenly jump to a new location
        // but slowly ease into it
        if (progress < 1f)
            progress = 1 - Mathf.Pow(progress - 1, 2);
        transform.position = Vector3.Lerp(_oldPosition, _moveDestination, progress);

        if (_elapsedTime > _dodgeCooldownTime)
            CheckForLasers();

        if (_elapsedTime > _firingCooldown)
        {
            ChangeState(State.Charging);
            return;
        }

        if (!_playerTransform) return;
        Vector3 targetDirection = _playerTransform.position - transform.position;
        float myCurrentAngle = transform.localRotation.eulerAngles.z - _startingZRotation;
        Vector3 myCurrentFacing = Quaternion.Euler(0, 0, myCurrentAngle) * Vector3.left;
        float angleToRotate = Vector3.SignedAngle(myCurrentFacing, targetDirection, Vector3.forward);
        float maxAngleSpeed = _maxRotationSpeed * Time.deltaTime;
        angleToRotate = Mathf.Clamp(angleToRotate, -maxAngleSpeed, maxAngleSpeed);
        transform.localRotation *= Quaternion.AngleAxis(angleToRotate, Vector3.forward);
    }

    void CheckForLasers()
    {
        GameObject[] lasers = GameObject.FindGameObjectsWithTag("Projectile");
        foreach(GameObject laser in lasers)
        {
            Transform laserTransform = laser.transform;
            float distance = Vector3.Distance(laserTransform.position, transform.position);
            if (distance < _laserDetectDistance && IsPositionInvalid(transform.position, laser.transform))
            {
                GetNewDestination(laser.transform);
            }
        }
    }

    void GetNewDestination(Transform incomingLaser)
    {
        do{ 
            _moveDestination = new Vector3(
                Random.Range(_movableArea.min.x, _movableArea.max.x),
                Random.Range(_movableArea.min.y, _movableArea.max.y),
                0);
        }
        while (IsPositionInvalid(_moveDestination, incomingLaser));

        _oldPosition = transform.position;
        _timeToMove = Vector3.Distance(_moveDestination, _oldPosition) / _mySpeed;
        _elapsedTime = 0;
    }

    bool IsPositionInvalid(Vector3 positionToTry, Transform incomingLaser)
    {
        // if there's no laser, all positions are valid
        if (incomingLaser == null) return false;

        Bounds myColliderBounds = _myCollider.bounds;
        myColliderBounds.center = positionToTry;
        myColliderBounds.Expand(_dodgingAmount);
        Ray laserRay = new Ray(incomingLaser.position, Vector3.right);
        return myColliderBounds.IntersectRay(laserRay);
    }

    void SetupCharge()
    {
        _chargeBall.SetActive(true);
        _chargeBall.transform.localScale = Vector3.zero;
        StartCoroutine(WarningFlashRoutine());
    }

    void DoCharge()
    {
        _elapsedTime += Time.deltaTime;
        float progress = _elapsedTime / _timeToCharge;
        Vector3 chargeBallScale = Vector3.Lerp(Vector3.zero, _maxChargeSize, progress);
        _chargeBall.transform.localScale = chargeBallScale;

        if (_elapsedTime >= _timeToCharge)
            ChangeState(State.Firing);
    }

    IEnumerator WarningFlashRoutine()
    {
        WaitForSeconds flashingWait = new WaitForSeconds(_warningFlashTime);
        while (_myState == State.Charging)
        {           
            _warningLine.SetActive(true);
            yield return flashingWait;
            _warningLine.SetActive(false);
            yield return flashingWait;
        }
    }

    void SetupFire()
    {
        _chargeBall.SetActive(false);
        _warningLine.SetActive(false);
        _myProjectile.SetActive(true);
        PlaySound(_projectileAudio);
    }

    void DoFire()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _beamDuration)
            ChangeState(State.Moving);
    }
}
