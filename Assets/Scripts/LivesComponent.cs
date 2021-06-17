using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivesComponent : MonoBehaviour
{
    [SerializeField]
    protected int _maxLives = 3;
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

    public bool IsDead { get; protected set; } = false;

    protected SpawnManager _spawnManager;
    protected UIManager _UIManager;

    protected virtual void Awake()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        if (!_spawnManager)
            Debug.LogError("Spawn Manager is null");

        _UIManager = FindObjectOfType<UIManager>();

        // if shield is null, get the shield object and color
        if (!_shieldObject)
            _shieldObject = FindShield();
        ToggleShields(false);

        GainLives(_maxLives);
    }
    // take 1 damage
    public void OnTakeDamage() 
    {
        OnTakeDamage(1);
    }
    // take "amount" damage
    public virtual void OnTakeDamage(int amount)
    {
        if (IsDead) return; // stops weird "double death" bugs from happening
        if (_shieldHP > 0) // Shields take damage
        {
            ShieldDamage();
            return;
        }

        GainLives(-amount);
    }
    // gain or lose life
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

    // turn on game objects that show damage
    void ToggleDamageEffects()
    {
        // loop through all damage effects
        for (int index = 0; index < _damageEffectObjects.Length; index++)
        {
            //ignore null effects
            if (_damageEffectObjects[index] == null) continue;

            // takes lives and max lives, and creates an index between 0 and
            // the number of damage objects
            int damageObjectsIndex = (_lives-1) * _damageEffectObjects.Length / (_maxLives-1);

            bool turnOn = damageObjectsIndex <= index;
            _damageEffectObjects[index].SetActive(turnOn);
        }
    }
    // Get a shield object
    GameObject FindShield()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.tag == "Shield")
            {
                _shieldColorOriginal = child.GetComponent<SpriteRenderer>().color;
                return child.gameObject;
            }
        }
        return null;
    }
    // shield takes damage
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
    // turn shield on or off
    public void ToggleShields(bool shieldOn)
    {
        if (!_shieldObject) return;
        _shieldObject.SetActive(shieldOn);
        if (shieldOn)
        {
            _shieldHP = _shieldMaxHP;
            _shieldObject.GetComponent<SpriteRenderer>().color = _shieldColorOriginal;
        }
    }
    // actions when this game object dies
    protected abstract void OnDeath();
}
