using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLives : EnemyLives
{
    CameraManager _cameraManager;
    MusicManager _musicManager;

    bool _secondPhase = false;
    [SerializeField]
    Sprite _secondPhaseSprite;
    [SerializeField]
    Transform _phaseExplosionPoints;

    [SerializeField]
    GameObject _phaseOneHitboxes;
    [SerializeField]
    GameObject _phaseTwoHitboxes;
    [SerializeField]
    GameObject _allPhaseOneObjects;
    [SerializeField]
    float _miniExplosionFrequency;
    [SerializeField]
    float _miniExplosionScale;
    [SerializeField]
    float _deathExplosionScale;
    [SerializeField]
    float _secondPhaseExplosionDuration;
    [SerializeField]
    float _deathExplosionDuration;

    protected override void Awake()
    {
        base.Awake();
        _cameraManager = Camera.main.GetComponent<CameraManager>();

        _musicManager = FindObjectOfType<MusicManager>();
        if (!_musicManager)
            Debug.LogError("Music Manager is null");
    }

    public override void OnTakeDamage(int amount)
    {
        base.OnTakeDamage(amount);
        _UIManager.UpdateBossLife((float)_lives / (float)_maxLives);
        if (_lives <= _maxLives / 2 && !_secondPhase)
            StartCoroutine(OnHalfHP());
    }

    protected override void OnDeath()
    {
        IsDead = true;
        StartCoroutine(DeathEffects());
    }

    IEnumerator OnHalfHP()
    {
        _secondPhase = true;
        BossBehavior myBehavior = GetComponent<BossBehavior>();
        myBehavior.GoToPhaseTwo();
        // other effects
        _cameraManager.CameraShake(0.1f, _secondPhaseExplosionDuration);
        yield return MiniExplosionsRoutine(_phaseOneHitboxes, _secondPhaseExplosionDuration);

        // big explosions
        for (int index = 0; index < _phaseExplosionPoints.childCount; index++)
        {
            Vector3 explodePosition = _phaseExplosionPoints.GetChild(index).position;
            Instantiate(_explosion, explodePosition, Quaternion.identity);
        }
        //change sprite after a short time
        yield return new WaitForSeconds(0.3f);
        _allPhaseOneObjects.SetActive(false);
        SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
        myRenderer.sprite = _secondPhaseSprite;
    }

    IEnumerator DeathEffects()
    {
        _UIManager.AddScore(_scoreValue);
        _musicManager.StopMusic(1);
        _cameraManager.CameraShake(0.2f, _deathExplosionDuration);
        yield return MiniExplosionsRoutine(_phaseTwoHitboxes, _deathExplosionDuration);

        _cameraManager.CameraShake(0.3f, 0.5f);
        GameObject explosion = Instantiate(_explosion, this.transform.position, Quaternion.identity);
        explosion.transform.localScale = new Vector3(_deathExplosionScale, _deathExplosionScale, 1);
        Destroy(this.gameObject, 0.3f);

        _UIManager.OnBossDeath();
        _musicManager.OnVictory();
    }

    IEnumerator MiniExplosionsRoutine(GameObject hitboxesToExplode, float duration)
    {
        float elapsedTime = 0;
        float nextExplosionTime = 0;
        float timeBetweenExplosions = 1 / _miniExplosionFrequency;
        while (elapsedTime < duration)
        {
            if (elapsedTime >= nextExplosionTime)
            {
                MiniExplosions(hitboxesToExplode);
                nextExplosionTime += timeBetweenExplosions;
            }
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }

    void MiniExplosions(GameObject hitboxesParent)
    {
        Vector3 explodeScale = new Vector3(_miniExplosionScale, _miniExplosionScale, 1);
        int hitboxCount = hitboxesParent.transform.childCount;
        for (int i = 0; i < hitboxCount; i++)
        {
            Collider2D hitbox = hitboxesParent.transform.GetChild(i).GetComponent<Collider2D>();
            Bounds hitboxArea = hitbox.bounds;
            Vector3 randomPoint = new Vector3(
                Random.Range(hitboxArea.min.x, hitboxArea.max.x),
                Random.Range(hitboxArea.min.y, hitboxArea.max.y), 0);
            GameObject newExplosion = Instantiate(_explosion, randomPoint, Quaternion.identity);
            newExplosion.transform.localScale = explodeScale;

            // make explosions quieter
            AudioSource explosionAudio = newExplosion.GetComponent<AudioSource>();
            if (explosionAudio)
                explosionAudio.volume *= _miniExplosionScale;
        }
    }
}
