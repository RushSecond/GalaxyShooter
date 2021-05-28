using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemy : Enemy
{
    protected override void SetupMovementType()
    {
        _myMovement = new SmartEnemyMovement(this, _enemyLaser);
    }

    protected override IEnumerator FireLaserRoutine()
    {
        yield return null;
    }
}
