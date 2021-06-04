using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RammingBehavior : EnemyBehavior
{
    enum State { Moving, Preparing, Ramming }
    State _myState;

    float _elapsedTime = 0;
    [SerializeField]
    float _minDistanceToRamPlayer = 4f;
    [SerializeField]
    float _timeToPrepare = 1f;
    float _rammingSpeed = 0f;
    [SerializeField]
    float _rammingAcceleration = 40f;
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
        ChangeState(State.Moving);
    }

    void ChangeState(State _newState)
    {
        _myState = _newState;
        _elapsedTime = 0;
        switch (_myState)
        {
            case State.Moving:
                transform.position = CameraManager.RandomPositionAtRight();
                transform.rotation = _myDefaultRotation;
                break;
            case State.Preparing:
                _targetDirectionToRam = _playerTransform.position - transform.position;
                _targetRotation = _myDefaultRotation * Quaternion.FromToRotation(Vector3.left, _targetDirectionToRam);
                break;
            case State.Ramming:
                _rammingSpeed = 0f;
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

    private void DoMove()
    {
        transform.position += Vector3.left * _mySpeed * Time.deltaTime;
        // check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer < _minDistanceToRamPlayer)            
            ChangeState(State.Preparing);
    }

    private void DoPrepare()
    {
        _elapsedTime += Time.deltaTime;
        float progress = _elapsedTime / _timeToPrepare;
        transform.rotation = Quaternion.Slerp(_myDefaultRotation, _targetRotation, progress);      
        if (_elapsedTime > _timeToPrepare)
            ChangeState(State.Ramming);
    }

    private void DoRam()
    {
        _rammingSpeed += _rammingAcceleration * Time.deltaTime;
        transform.position += _rammingSpeed * Time.deltaTime * _targetDirectionToRam;
        if (transform.position.x <= -CameraManager.GetCameraBounds().extents.x - 5f)
            ChangeState(State.Moving);
    }
}
