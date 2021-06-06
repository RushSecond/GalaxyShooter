using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgingBehavior : EnemyBehavior
{ 
    [SerializeField]
    Bounds _movableArea;
    [SerializeField]
    private float _dodgingAmount = 0.2f;
    [SerializeField]
    private float _laserDetectDistance = 5f;
    [SerializeField]
    private float dodgeCooldownTime = 3f;

    Vector3 _oldPosition;
    Vector3 _moveDestination;
    float _timeToMove;
    float _elapsedMoveTime;
    Transform _playerTransform;
    Collider2D _myCollider;

    protected override void Start()
    {
        base.Start();
        _playerTransform = FindObjectOfType<Player>().transform;
        transform.position = CameraManager.RandomPositionAtRight();
        _myCollider = GetComponent<Collider2D>();

        GetNewDestination(null);
    }

    public override void Act()
    {
        _elapsedMoveTime += Time.deltaTime;
        float progress = _elapsedMoveTime / _timeToMove;
        // use function 1-(x-1)^2 so we can suddenly jump to a new location
        // but slowly ease into it
        if (progress < 1f)
            progress = 1 - Mathf.Pow(progress - 1, 2);
        transform.position = Vector3.Lerp(_oldPosition, _moveDestination, progress);

        if (_elapsedMoveTime > dodgeCooldownTime)
            CheckForLasers();
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
        _elapsedMoveTime = 0;
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
}
