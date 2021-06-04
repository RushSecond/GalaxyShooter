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
    Quaternion _targetRotation;
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
        // check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer < _minDistanceToRamPlayer)            
            ChangeState(State.Preparing);
    }

    private void SetupPrepare()
    {
        _targetDirectionToRam = _playerTransform.position - transform.position;
        _targetRotation = _myDefaultRotation * Quaternion.FromToRotation(Vector3.left, _targetDirectionToRam);
    }

    private void DoPrepare()
    {
        _elapsedTime += Time.deltaTime;
        float progress = _elapsedTime / _timeToPrepare;
        transform.rotation = Quaternion.Slerp(_myDefaultRotation, _targetRotation, progress);      
        if (_elapsedTime > _timeToPrepare)
            ChangeState(State.Ramming);
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
