using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLives : EnemyLives
{
    public override void OnTakeDamage(int amount)
    {
        base.OnTakeDamage(amount);
        _UIManager.UpdateBossLife((float)_lives / (float)_maxLives);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        _UIManager.OnBossDeath();
    }
}
