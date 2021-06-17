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
        public virtual void EndState()
        { }
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
        [SerializeField]
        float _moveSpeedPhaseTwo;
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
            float speed = _myBehavior.onPhaseTwo ? _moveSpeedPhaseTwo : _moveSpeed;
            _timeToMove = Vector3.Distance(_startingPoint, _movementEndPoint) / speed;
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
        [System.Serializable]
        struct BeamObject
        {
            [SerializeField]
            public GameObject _beamParent;
            [SerializeField]
            public GameObject _beamAttack;
            [SerializeField]
            public GameObject _chargeBall;
            [SerializeField]
            public GameObject _warningLine;
        }

        enum BeamAttackState { Tracking, Locked, Fire }
        BeamAttackState _currentState;

        Quaternion _targetRotation;

        [SerializeField]
        BeamObject[] _beamObjects;
        float _defaultRotation;
        Vector3 _maxChargeSize;
        [SerializeField]
        float _trackingDuration;
        [SerializeField]
        float _trackingDurationPhaseTwo;
        [SerializeField]
        float _trackingFlashTime;
        [SerializeField]
        float _lockedOnDuration;
        [SerializeField]
        float _lockedOnDurationPhaseTwo;
        [SerializeField]
        float _lockedFlashTime;
        [SerializeField]
        float _firingDuration;
        [SerializeField]
        int _beamVolleysPhaseTwo;
        int _beamVolleysFired;

        public override void Setup(BossBehavior myBehavior)
        {
            base.Setup(myBehavior);
            _maxChargeSize = _beamObjects[0]._chargeBall.transform.localScale;
            ChangeState(BeamAttackState.Tracking);
            _defaultRotation = _beamObjects[0]._beamParent.transform.eulerAngles.z;
            _beamVolleysFired = 0;
        }

        public override void EndState()
        {
            foreach (BeamObject obj in _beamObjects)
            {
                obj._beamAttack.SetActive(false);
                RotateToVector(obj._beamParent, Vector3.left, _defaultRotation);
            }
        }

        void ChangeState(BeamAttackState newState)
        {
            _elapsedTime = 0;
            _currentState = newState;
            if (_currentState == BeamAttackState.Tracking)
                SetupTracking();

            if (_currentState == BeamAttackState.Fire)
                FireBeams();
        }

        public override void Act()
        {
            _elapsedTime += Time.deltaTime;
            switch ((int)_currentState)
            {
                case 0:
                    DoTracking();
                    break;
                case 1:
                    DoLocked();
                    break;
                case 2:
                    DoFire();
                    break;
            }
        }

        void SetupTracking()
        {
            foreach (BeamObject obj in _beamObjects)
            {
                obj._chargeBall.SetActive(true);
                obj._chargeBall.transform.localScale = Vector3.zero;
                _myBehavior.StartCoroutine(WarningFlashRoutine(obj._warningLine));
            }
        }

        void DoTracking()
        {
            float timetoTrack = _myBehavior.onPhaseTwo ? _trackingDurationPhaseTwo : _trackingDuration;
            float progress = _elapsedTime / timetoTrack;
            Vector3 chargeBallScale = Vector3.Lerp(Vector3.zero, _maxChargeSize, progress);
            foreach (BeamObject obj in _beamObjects)
            {
                obj._chargeBall.transform.localScale = chargeBallScale;
            }

            if (_myBehavior._playerTransform)
                RotateBeams();
            
            if (_elapsedTime >= timetoTrack)
                ChangeState(BeamAttackState.Locked);
        }

        void RotateBeams()
        {
            foreach (BeamObject obj in _beamObjects)
            {
                Vector3 directionToPlayer = _myBehavior._playerTransform.position - obj._beamParent.transform.position;
                RotateToVector(obj._beamParent, directionToPlayer, _defaultRotation);
            }
        }

        void DoLocked()
        {
            float timetoLock = _myBehavior.onPhaseTwo ? _lockedOnDurationPhaseTwo : _lockedOnDuration;
            if (_elapsedTime >= timetoLock)
                ChangeState(BeamAttackState.Fire);
        }

        void DoFire()
        {
            if (_elapsedTime < _firingDuration) return;
            if (_myBehavior.onPhaseTwo && _beamVolleysFired < _beamVolleysPhaseTwo)
            {
                ChangeState(BeamAttackState.Tracking);
                TurnOffBeams();
            }
            else
                _myBehavior.SwitchState(_myBehavior.laserTrackingState);
        }

        void FireBeams()
        {
            _beamVolleysFired++;
            foreach (BeamObject obj in _beamObjects)
            {
                obj._chargeBall.SetActive(false);
                obj._beamAttack.SetActive(true);
            }
            _myBehavior.PlaySound(_myBehavior._projectileAudio, 0.4f);
        }

        void TurnOffBeams()
        {
            foreach (BeamObject obj in _beamObjects)
            {
                obj._beamAttack.SetActive(false);
            }
        }

        IEnumerator WarningFlashRoutine(GameObject warningLine)
        {
            WaitForSeconds slowFlash = new WaitForSeconds(_trackingFlashTime);
            WaitForSeconds fastFlash = new WaitForSeconds(_lockedFlashTime);
            while (_currentState != BeamAttackState.Fire && !_myBehavior._myEnemy.enemyLives.IsDead)
            {               
                yield return _currentState == BeamAttackState.Locked ? fastFlash : slowFlash;
                warningLine.SetActive(true);            
                yield return _currentState == BeamAttackState.Locked ? fastFlash : slowFlash;
                warningLine.SetActive(false);
            }
        }
    }

    [System.Serializable]
    class LaserTrackingAttack : BossState
    {
        [SerializeField]
        Transform[] _firstTurretSet;
        [SerializeField]
        Transform[] _secondTurretSet;
        [SerializeField]
        Vector3 _movementEndPoint;
        [SerializeField]
        float _moveSpeed;
        [SerializeField]
        float _moveSpeedPhaseTwo;
        float _timeToMove;
        [SerializeField]
        int _numberOfVolleys;
        float _timeBetweenVolleys;
        int _volleysFired;
        float _nextVolleyTime;

        public override void Setup(BossBehavior myBehavior)
        {
            base.Setup(myBehavior);
            float speed = _myBehavior.onPhaseTwo ? _moveSpeedPhaseTwo : _moveSpeed;
            _timeToMove = Vector3.Distance(_startingPoint, _movementEndPoint) / speed;
            _timeBetweenVolleys = _timeToMove / (_numberOfVolleys + 1);
            _nextVolleyTime = _timeBetweenVolleys;
            _volleysFired = 0;
        }

        public override void Act()
        {
            _elapsedTime += Time.deltaTime;
            float progress = _elapsedTime / _timeToMove;
            _myTransform.position = Vector3.Lerp(_startingPoint, _movementEndPoint, progress);

            if (progress >= 1f)
            {
                _myBehavior.SwitchState(_myBehavior.missileState);
                return;
            }
            if (_elapsedTime >= _nextVolleyTime)
            {
                _volleysFired++;
                // use first turret set odds, second turret set on evens
                Transform[] turretsToFire = _volleysFired % 2 == 1 ? _firstTurretSet : _secondTurretSet;
                FireLaserVolley(turretsToFire);
                _nextVolleyTime += _timeBetweenVolleys;
            }
        }

        void FireLaserVolley(Transform[] turretsToFire)
        {
            Vector3 direction;
            if (!_myBehavior._playerTransform)
                direction = Vector3.left;
            else
                direction = _myBehavior._playerTransform.position - _myTransform.position;
            foreach(Transform turret in turretsToFire)
            {
                _myBehavior.FireProjectile(direction, turret.localPosition);
            }
        }
    }

    [System.Serializable]
    class MissilesAttack : BossState
    {
        [SerializeField]
        GameObject _myMissile;
        [SerializeField]
        Transform[] _missileBays;
        [SerializeField]
        float _waitTime;
        int missilesFired;

        public override void Act()
        {
            _elapsedTime += Time.deltaTime;
            if (missilesFired < 2 && _elapsedTime >= _waitTime / 2 && _myBehavior.onPhaseTwo)
            {
                FireMissiles();
            }
            if (_elapsedTime >= _waitTime)
                _myBehavior.SwitchState(_myBehavior.laserFanState);
        }

        public override void Setup(BossBehavior myBehavior)
        {
            base.Setup(myBehavior);
            missilesFired = 0;
            FireMissiles();
        }

        void FireMissiles()
        {
            missilesFired++;
            foreach (Transform missileBay in _missileBays)
            {
                Instantiate(_myMissile, missileBay.position, _myMissile.transform.rotation);
            }
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
    Transform _playerTransform;

    bool onPhaseTwo = false;
    public void GoToPhaseTwo()
    {
        onPhaseTwo = true;
    }

    protected override void Start()
    {
        base.Start();
        _playerTransform = FindObjectOfType<Player>().transform;
        SwitchState(moveOnScreenState);
    }

    void SwitchState(BossState newState)
    {
        if (_currentState != null) _currentState.EndState();
        _currentState = newState;
        _currentState.Setup(this);
    }

    public override void Act()
    {
        _currentState.Act();
    }
}
