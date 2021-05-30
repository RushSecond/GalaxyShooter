using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivesComponent : MonoBehaviour
{
    [SerializeField]
    private int _maxLives = 3;
    protected int _lives = 0;
    protected int _shieldHP = 0;
    [SerializeField]
    private int _shieldMaxHP = 3;
    [SerializeField]
    private GameObject _shieldObject;
    [SerializeField]
    private Color _shieldColorOriginal;
    [SerializeField]
    private GameObject[] _damageEffectObjects;
    [SerializeField]
    protected GameObject _explosion;

    public bool isDead { get; protected set; } = false;

    protected SpawnManager _spawnManager;
    protected UIManager _UIManager;

    protected virtual void Start()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");

        _UIManager = FindObjectOfType<UIManager>();

        GainLives(_maxLives);
    }

    public void OnTakeDamage()
    {
        OnTakeDamage(1);
    }

    public virtual void OnTakeDamage(int amount)
    {
        if (_shieldHP > 0) // Shields take damage
        {
            ShieldDamage();
            return;
        }

        GainLives(-amount);
    }

    protected virtual void GainLives(int livesGained)
    {
        // new lives should be clamped between 0 and max
        _lives = Mathf.Clamp(_lives + livesGained, 0, _maxLives);

        if (_lives == 0)
        {
            OnDeath();
            return;
        }

        ToggleDamageEffects();
    }

    void ToggleDamageEffects()
    {
        // loop through all damage effects
        for (int index = 0; index < _damageEffectObjects.Length; index++)
        {
            //ignore null effects
            if (_damageEffectObjects[index] == null) continue;

            // Effects with an index less than or equal to "lives -1" should be turned on
            // All others turned off
            bool turnOn = _lives - 1 <= index;
            _damageEffectObjects[index].SetActive(turnOn);
        }
    }

    void ShieldDamage()
    {
        _shieldHP--;
        if (_shieldHP == 0)
        {
            ToggleShields(false);
            return;
        }

        // Change the color of the shield object to be less bright 
        float fractionOfShieldRemaining = (float)_shieldHP / (float)_shieldMaxHP;
        Color newShieldColor = Color.Lerp(Color.black, _shieldColorOriginal, fractionOfShieldRemaining);
        _shieldObject.GetComponent<SpriteRenderer>().color = newShieldColor;
    }

    public void ToggleShields(bool shieldOn)
    {
        _shieldObject.SetActive(shieldOn);
        if (shieldOn)
        {
            _shieldHP = _shieldMaxHP;
            _shieldObject.GetComponent<SpriteRenderer>().color = _shieldColorOriginal;
        }
    }

    protected abstract void OnDeath();
}
