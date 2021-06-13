using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BossBehavior : EnemyBehavior
{
    abstract class BossState
    {
        protected Transform _myTransform;
        protected BossBehavior _myBehavior;
        protected float _elapsedTime;
        protected Vector3 _startingPoint;

        public virtual void Setup(BossBehavior myBehavior)
        {
            _myBehavior = myBehavior;
            _myTransform = myBehavior.transform;
            _elapsedTime = 0;
            _startingPoint = _myTransform.position;
        }
        public abstract void Act();
    }

    [System.Serializable]
    class MoveOnScreen : BossState
    {
        [SerializeField]
        float _distanceAppearOffCamera;
        [SerializeField]
        Vector3 _movementEndPoint;
        [SerializeField]
        float _moveSpeed;
        float _timeToMove;     

        public override void Act()
        {
            _elapsedTime += Time.deltaTime;
            float progress = _elapsedTime / _timeToMove;
            _myTransform.position = Vector3.Lerp(_startingPoint, _movementEndPoint, progress);
            if (progress >= 1f)
            {
                _myBehavior.SwitchState(_myBehavior.laserFanState);
            }
        }

        public override void Setup(BossBehavior myBehavior)
        {
            base.Setup(myBehavior);
            float xPosition = CameraManager.GetCameraBounds().max.x + _distanceAppearOffCamera;
            float yPosition = _movementEndPoint.y;
            _startingPoint = new Vector3(xPosition, yPosition);
            _myTransform.position = _startingPoint;
            _timeToMove = Vector3.Distance(_startingPoint, _movementEndPoint) / _moveSpeed;
        }
    }

    [System.Serializable]
    class LaserFanAttack : BossState
    {
        [SerializeField]
        Vector3 _movementEndPoint;
        [SerializeField]
        float _moveSpeed;
        float _timeToMove;
        [SerializeField]
        int _numberOfVolleys;
        float _timeBetweenVolleys;
        int _volleysFired;
        float _nextVolleyTime;
        [SerializeField]
        int _firstVolleyLaserCount;
        [SerializeField]
        int _secondVolleyLaserCount;
        [SerializeField]
        int _volleyLaserSpread;

        public override void Act()
        {
            _elapsedTime += Time.deltaTime;
            float progress = _elapsedTime / _timeToMove;
            _myTransform.position = Vector3.Lerp(_startingPoint, _movementEndPoint, progress);
           
            if (progress >= 1f)
            {
                _myBehavior.SwitchState(_myBehavior.beamAttackState);
                return;
            }
            if (_elapsedTime >= _nextVolleyTime)
            {
                _volleysFired++;
                // shoot first volley number on odds, second volley number on evens
                int lasersToFire = _volleysFired % 2 == 1 ? _firstVolleyLaserCount : _secondVolleyLaserCount;
                FireLaserVolley(lasersToFire);
                _nextVolleyTime += _timeBetweenVolleys;
            }
        }

        public override void Setup(BossBehavior myBehavior)
        {
            base.Setup(myBehavior);
            _timeToMove = Vector3.Distance(_startingPoint, _movementEndPoint) / _moveSpeed;
            _timeBetweenVolleys = _timeToMove / (_numberOfVolleys+1);
            _nextVolleyTime = _timeBetweenVolleys;
            _volleysFired = 0;
        }

        void FireLaserVolley(int laserCount)
        {
            int angleBetweenLasers = 2 * _volleyLaserSpread / (laserCount - 1);
            for (int angle = -_volleyLaserSpread; angle <= _volleyLaserSpread; angle += angleBetweenLasers)
            {
                _myBehavior.FireProjectile(angle + 180);
            }
        }
    }

    [System.Serializable]
    class BeamAttack : BossState
    {
        [SerializeField]
        GameObject[] beamObjects;

        public override void Act()
        {
        }

        public override void Setup(BossBehavior myBehavior)
        {
        }
    }

    [System.Serializable]
    class LaserTrackingAttack : BossState
    {
        public override void Act()
        {
        }

        public override void Setup(BossBehavior myBehavior)
        {
        }
    }

    [System.Serializable]
    class MissilesAttack : BossState
    {
        public override void Act()
        {
        }

        public override void Setup(BossBehavior myBehavior)
        {
        }
    }

    [SerializeField]
    MoveOnScreen moveOnScreenState;
    [SerializeField]
    LaserFanAttack laserFanState;
    [SerializeField]
    BeamAttack beamAttackState;
    [SerializeField]
    LaserTrackingAttack laserTrackingState;
    [SerializeField]
    MissilesAttack missileState;

    BossState _currentState;

    protected override void Start()
    {
        base.Start();
        SwitchState(moveOnScreenState);
    }

    void SwitchState(BossState newState)
    {
        _currentState = newState;
        _currentState.Setup(this);
    }

    public override void Act()
    {
        _currentState.Act();
    }
}
