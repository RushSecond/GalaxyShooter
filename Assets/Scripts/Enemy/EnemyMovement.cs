using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement
{
    protected Enemy _myEnemy;
    protected Transform _myTransform;

    public EnemyMovement(Enemy enemyScript)
    {
        _myEnemy = enemyScript;
        _myTransform = enemyScript.transform;
        SetStartPosition();
    }

    protected virtual void SetStartPosition()
    {
        _myTransform.position = SpawnManager.RandomPositionAtRight();
    }

    public virtual void Move(float speed)
    {
        _myTransform.Translate(Vector3.down * speed * Time.deltaTime);

        if (_myTransform.position.x <= -CameraManager.GetCameraBounds().extents.x - 1f)
        {
            _myTransform.position = SpawnManager.RandomPositionAtRight();
        }
    }
}
