using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyBehavior : EnemyBehavior
{
    protected override void Start()
    {
        base.Start();
        transform.position = SpawnManager.RandomPositionAtRight();
        StartCoroutine(FireLaserRoutine());
    }

    public override void Act()
    {
        transform.Translate(Vector3.down * _mySpeed * Time.deltaTime);

        if (transform.position.x <= -CameraManager.GetCameraBounds().extents.x - 1f)
        {
            transform.position = SpawnManager.RandomPositionAtRight();
        }
    }

    protected IEnumerator FireLaserRoutine()
    {
        while (true)
        {
            int fireTime = Random.Range(3, 8);
            yield return new WaitForSeconds(fireTime);

            if (_myEnemy.enemyLives.isDead) break;
            FireProjectile(transform.rotation);
        }
    }
}