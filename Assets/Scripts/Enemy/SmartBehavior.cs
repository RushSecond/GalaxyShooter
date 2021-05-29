using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBehavior : EnemyBehavior
{
    float elapsedTime = 0;
    enum MovingState { Moving, Firing, Waiting }
    MovingState _myState = MovingState.Moving;

    Vector3[] _wayPoints;
    int _currentMoveIndes = 0;
    Vector3 _oldPosition;
    float _timeToMove = -1f;
    
    [SerializeField]
    float _timeToFire = 0.5f;
    [SerializeField]
    float _timetoWait = 0.5f;
    Transform _playerTransform;

    protected override void Start()
    {
        base.Start();
        _wayPoints = new Vector3[4];
        _wayPoints[0] = new Vector3(Random.Range(4f, 7f), 0, 0);
        _wayPoints[1] = _wayPoints[0] + new Vector3(-2, 4, 0);
        _wayPoints[2] = _wayPoints[0];
        _wayPoints[3] = _wayPoints[0] + new Vector3(-2, -4, 0);

        transform.position = new Vector3(CameraManager.GetCameraBounds().extents.x + 1f, 0, 0);
        _oldPosition = transform.position;
        _playerTransform = FindObjectOfType<Player>().transform;
    }

    void ChangeState(MovingState _newState)
    {
        _myState = _newState;
        if (_myState == MovingState.Moving)
        {
            _oldPosition = transform.position;
            _currentMoveIndes = ++_currentMoveIndes % _wayPoints.Length;
            _timeToMove = -1;
        }
        elapsedTime = 0;
    }

    public override void Act()
    {
        switch((int)_myState)
        {
            case 0:
                DoMove();
                break;
            case 1:
                DoFire();
                break;
            case 2:
                DoWait();
                break;
        }
    }

    void DoMove()
    {
        if (_timeToMove < 0)
        {
            float distance = Vector3.Distance(_oldPosition, _wayPoints[_currentMoveIndes]);
            _timeToMove = distance / _mySpeed;
        }
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / _timeToMove;
        transform.position = Vector3.Slerp(_oldPosition, _wayPoints[_currentMoveIndes], progress);

        if (elapsedTime >= _timeToMove) ChangeState(MovingState.Firing);
    }

    void DoFire()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= _timeToFire)
        {
            Vector3 vectorToPlayer = transform.position - _playerTransform.position;
            float angleToFire = Vector3.SignedAngle(Vector3.up, vectorToPlayer, Vector3.forward);
            Quaternion playerDirection = Quaternion.Euler(0, 0, angleToFire);
            FireProjectile(playerDirection);
            ChangeState(MovingState.Waiting);
        }
    }

    void DoWait()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= _timetoWait)
        {
            ChangeState(MovingState.Moving);
        }
    }
}
