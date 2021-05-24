using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement
{
    protected Transform _myTransform;

    public EnemyMovement(Enemy enemyScript)
    {
        _myTransform = enemyScript.transform;
        _myTransform.position = SpawnManager.RandomPositionAtRight();
    }

    public virtual void Move(float speed)
    {
        _myTransform.Translate(Vector3.down * speed * Time.deltaTime);

        if (_myTransform.position.x <= -SpawnManager._screenBoundsX)
        {
            _myTransform.position = SpawnManager.RandomPositionAtRight();
        }
    }
}
