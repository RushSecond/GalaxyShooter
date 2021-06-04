using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyBehavior : EnemyBehavior
{
    protected override void Start()
    {
        base.Start();
        transform.position = CameraManager.RandomPositionAtRight();
        StartCoroutine(FireLaserRoutine());
    }

    public override void Act()
    {
        transform.position += Vector3.left * _mySpeed * Time.deltaTime;

        if (transform.position.x <= -CameraManager.GetCameraBounds().extents.x - 1f)
        {
            transform.position = CameraManager.RandomPositionAtRight();
        }
    }

    protected IEnumerator FireLaserRoutine()
    {
        while (true)
        {
            int fireTime = Random.Range(3, 8);
            yield return new WaitForSeconds(fireTime);

            if (_myEnemy.enemyLives.IsDead) break;
            FireProjectile(Quaternion.Euler(0, 0, -90f));
        }
    }
}