using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RammingBehavior : EnemyBehavior
{
    enum State { Moving, Preparing, Ramming, Reappear }
    State _myState;

    float _elapsedTime = 0;
    [SerializeField]
    float _minDistanceToRamPlayer = 4f;
    [SerializeField]
    float _timeToPrepare = 1f;
    [SerializeField]
    GameObject _warningLine;
    [SerializeField]
    float _warningFlashTime;
    float _rammingSpeed = 0f;
    [SerializeField]
    float _rammingAcceleration = 40f;
    [SerializeField]
    Transform _thrusterTransform;
    [SerializeField]
    float _thrusterSizeRatioPerSecond = 2f;
    [SerializeField]
    AudioClip _thrusterSound;
    Vector3 _thrusterIntitialScale;
    [SerializeField]
    float _timeToReappear = 1f;

    Vector3 _targetDirectionToRam;
    float _angleToRotate;
    Transform _playerTransform;
    Quaternion _myDefaultRotation;

    protected override void Start()
    {
        base.Start();
        transform.position = CameraManager.RandomPositionAtRight();
        _playerTransform = FindObjectOfType<Player>().transform;
        _myDefaultRotation = Quaternion.Euler(0, 0, _startingZRotation);
        _thrusterIntitialScale = _thrusterTransform.localScale;
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
                SetupPrepare();
                break;
            case 2:
                SetupRamming();
                break;
            case 3:
                StartCoroutine(ReappearRoutine());
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
                DoPrepare();
                break;
            case 2:
                DoRam();
                break;
        }
    }

    void SetupMove()
    {
        _thrusterTransform.localScale = _thrusterIntitialScale;
        transform.position = CameraManager.RandomPositionAtRight();
        transform.rotation = _myDefaultRotation;
    }

    private void DoMove()
    {
        transform.position += Vector3.left * _mySpeed * Time.deltaTime;
        // if the enemy goes off the screen to the left, it should reappear on the right
        if (transform.position.x <= -CameraManager.GetCameraBounds().extents.x - 1f)
            transform.position = CameraManager.RandomPositionAtRight();
        // check distance to player
        if (!_playerTransform) return;
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer < _minDistanceToRamPlayer)            
            ChangeState(State.Preparing);      
    }

    private void SetupPrepare()
    {
        _targetDirectionToRam = _playerTransform.position - transform.position;
        float myCurrentAngle = transform.rotation.eulerAngles.z - _startingZRotation;
        Vector3 myCurrentFacing = Quaternion.Euler(0, 0, myCurrentAngle) * Vector3.left;
        _angleToRotate = Vector3.SignedAngle(myCurrentFacing, _targetDirectionToRam, Vector3.forward);
        StartCoroutine(WarningLineRoutine());
    }

    private void DoPrepare()
    {
        _elapsedTime += Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(_angleToRotate / _timeToPrepare * Time.deltaTime, Vector3.forward);
        RotateToVector(_warningLine, _targetDirectionToRam, 180);
        if (_elapsedTime > _timeToPrepare)
            ChangeState(State.Ramming);
    }

    IEnumerator WarningLineRoutine()
    {
        WaitForSeconds flashingWait = new WaitForSeconds(_warningFlashTime);
        while (_myState == State.Preparing)
        {
            yield return flashingWait;
            _warningLine.gameObject.SetActive(true);
            yield return flashingWait;
            _warningLine.gameObject.SetActive(false);           
        }
    }

    private void SetupRamming()
    {
        PlaySound(_thrusterSound);
        _rammingSpeed = 0f;
    }

    private void DoRam()
    {
        _elapsedTime += Time.deltaTime;
        // accelerate and then move
        _rammingSpeed += _rammingAcceleration * Time.deltaTime;
        transform.position += _rammingSpeed * Time.deltaTime * _targetDirectionToRam;
        // make the thruster larger
        Vector3 newThrusterScale = (1f + _thrusterSizeRatioPerSecond * _elapsedTime) * _thrusterIntitialScale;
        _thrusterTransform.localScale = newThrusterScale;
        if (!CameraManager.IsInsideCameraBounds(transform.position, 5f))
            ChangeState(State.Reappear);
    }

    private IEnumerator ReappearRoutine()
    {
        yield return new WaitForSeconds(_timeToReappear);
        ChangeState(State.Moving);
    }
}
