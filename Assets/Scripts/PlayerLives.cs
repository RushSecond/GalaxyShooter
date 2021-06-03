using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLives : LivesComponent
{
    private CameraManager _cameraManager; 

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        _cameraManager = Camera.main.GetComponent<CameraManager>();
        if (!_cameraManager)
            Debug.LogError("Camera Manager is null");
    }

    public override void OnTakeDamage(int amount)
    {
        if (_shieldHP <= 0) _cameraManager.CameraShake();
        base.OnTakeDamage(amount);       
    }

    protected override void GainLives(int livesGained)
    {
        base.GainLives(livesGained);
        _UIManager.UpdateLives(_lives);
    }

    protected override void OnDeath()
    {
        IsDead = true;
        _spawnManager.OnPlayerDeath();

        //Create explosion and destroy player
        Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(gameObject, 0.3f);
    }

    public void RepairPowerup()
    {
        GainLives(1);
    }
}
