using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBehavior : EnemyBehavior
{  
    enum MovingState { Moving, Firing, Waiting }
    MovingState _myState;

    Vector3[] _wayPoints; // where this enemy will move
    int _currentMoveIndes = -1;
    Vector3 _oldPosition;
    float _timeToMove;

    float elapsedTime = 0;
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
        _playerTransform = FindObjectOfType<Player>().transform;
        ChangeState(MovingState.Moving);
    }

    void ChangeState(MovingState _newState)
    {
        _myState = _newState;
        if (_myState == MovingState.Moving)
        {
            _oldPosition = transform.position;
            _currentMoveIndes = ++_currentMoveIndes % _wayPoints.Length;
            float distance = Vector3.Distance(_oldPosition, _wayPoints[_currentMoveIndes]);
            _timeToMove = distance / _mySpeed;
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
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / _timeToMove;
        progress = Mathf.SmoothStep(0, 1, progress);
        transform.position = Vector3.Lerp(_oldPosition, _wayPoints[_currentMoveIndes], progress);

        if (elapsedTime >= _timeToMove) ChangeState(MovingState.Firing);
    }

    void DoFire()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= _timeToFire && _playerTransform)
        {
            Vector3 vectorToPlayer = _playerTransform.position - transform.position;
            float angleToFire = Vector3.SignedAngle(Vector3.right, vectorToPlayer, Vector3.forward);
            FireProjectile(angleToFire);
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
