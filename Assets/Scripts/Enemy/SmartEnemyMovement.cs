using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemyMovement : EnemyMovement
{
    float elapsedTime = 0;
    enum MovingState { Moving, Firing, Waiting }
    MovingState _myState = MovingState.Moving;

    Vector3[] _wayPoints;
    int _currentMoveIndes = 0;
    Vector3 _oldPosition;
    float _timeToMove = -1f;
    
    float _timeToFire = 0.5f;
    GameObject _myLaser;
    Transform _playerTransform;

    float _timetoWait = 0.5f;

    public SmartEnemyMovement(Enemy enemyScript, GameObject laser) : base(enemyScript)
    {
        _myLaser = laser;
        _playerTransform = GameObject.FindObjectOfType<Player>().transform;
    }

    protected override void SetStartPosition()
    {
        _wayPoints = new Vector3[4];
        _wayPoints[0] = new Vector3(Random.Range(4f, 7f), 0, 0);
        _wayPoints[1] = _wayPoints[0] + new Vector3(-2, 4, 0);
        _wayPoints[2] = _wayPoints[0];
        _wayPoints[3] = _wayPoints[0] + new Vector3(-2, -4, 0);

        _myTransform.position = new Vector3(CameraManager.GetCameraBounds().extents.x + 1f, 0, 0);
        _oldPosition = _myTransform.position;
    }

    void ChangeState(MovingState _newState)
    {
        _myState = _newState;
        if (_myState == MovingState.Moving)
        {
            _oldPosition = _myTransform.position;
            _currentMoveIndes = ++_currentMoveIndes % _wayPoints.Length;
            _timeToMove = -1;
        }
        elapsedTime = 0;
    }

    public override void Move(float speed)
    {
        switch((int)_myState)
        {
            case 0:
                DoMove(speed);
                break;
            case 1:
                DoFire();
                break;
            case 2:
                DoWait();
                break;
        }
    }

    void DoMove(float speed)
    {
        if (_timeToMove < 0)
        {
            float distance = Vector3.Distance(_oldPosition, _wayPoints[_currentMoveIndes]);
            _timeToMove = distance / speed;
        }
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / _timeToMove;
        _myTransform.position = Vector3.Slerp(_oldPosition, _wayPoints[_currentMoveIndes], progress);

        if (elapsedTime >= _timeToMove) ChangeState(MovingState.Firing);
    }

    void DoFire()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= _timeToFire)
        {
            Vector3 vectorToPlayer = _myTransform.position - _playerTransform.position;
            float angleToFire = Vector3.SignedAngle(Vector3.up, vectorToPlayer, Vector3.forward);
            Quaternion playerDirection = Quaternion.Euler(0, 0, angleToFire);
            GameObject.Instantiate(_myLaser, _myTransform.position, playerDirection);
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
