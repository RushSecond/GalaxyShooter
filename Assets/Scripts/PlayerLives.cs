using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLives : LivesComponent
{
    private CameraManager _cameraManager;
    private MusicManager _musicManager;
    AudioSource _hurtSoundSource;

    [Header("Invincibility")]
    [SerializeField]
    private float _invincibleDuration;
    [SerializeField]
    private float _invincibleAlpha;
    [SerializeField]
    private float _invincibleFadeTime;

    [Header("HitAudio")]
    [SerializeField]
    private AudioClip _hurtAudioClip;
    [SerializeField]
    private float _hurtAudioDuration;
    [SerializeField]
    private float _hurtAudioVolume;
    [SerializeField]
    private float _hurtAudioPitch;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        _cameraManager = Camera.main.GetComponent<CameraManager>();
        if (!_cameraManager)
            Debug.LogError("Camera Manager is null");

        _musicManager = FindObjectOfType<MusicManager>();
        if (!_musicManager)
            Debug.LogError("Music Manager is null");

        Collider2D _myCollider = GetComponent<Collider2D>();
        if (!_myCollider)
            Debug.LogError("Player collider is null");
    }

    public override void OnTakeDamage(int amount)
    {
        if (_shieldHP <= 0) _cameraManager.CameraShake();
        base.OnTakeDamage(amount);
    }

    protected override void GainLives(int livesGained)
    {
        base.GainLives(livesGained);
        _UIManager.UpdatePlayerLives(_lives);
        if (livesGained < 0)
        {
            _cameraManager.CameraShake();
            if (_lives > 0) StartCoroutine(PlayerHurtRoutine());
        }       
    }

    protected override void OnDeath()
    {
        IsDead = true;
        _musicManager.OnPlayerDeath();
        //Create explosion and destroy player
        Instantiate(_explosion, transform.position, Quaternion.identity);
        Destroy(gameObject, 0.3f);
    }

    public void RepairPowerup()
    {
        GainLives(1);
    }

    IEnumerator PlayerHurtRoutine()
    {
        PlayHurtSound();
        float elapsedTime = 0;
        // turn off collision
        Collider2D myCollider = GetComponent<Collider2D>();
        myCollider.enabled = false;
        // get sprite renderer to fade in and out
        SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
        Color myColor = myRenderer.color;
        Color myAlphaColor = new Color(myColor.r, myColor.g, myColor.b, _invincibleAlpha);
        while (elapsedTime < _invincibleDuration)
        {
            // use sin squared curve to blend
            elapsedTime += Time.deltaTime;
            float lerpProgress = Mathf.Pow(Mathf.Sin(elapsedTime * (Mathf.PI / 2) / _invincibleFadeTime), 2);
            myRenderer.color = Color.Lerp(myColor, myAlphaColor, lerpProgress);
            yield return null;         
        }

        myCollider.enabled = true; // turn collision back on
        myRenderer.color = myColor; // alpha back to default
    }

    void PlayHurtSound()
    {
        // don't play sound if it already is playing
        if (_hurtSoundSource != null) return;
        _hurtSoundSource = AudioHelper.PlayClipAtCamera(
            _hurtAudioClip, _hurtAudioDuration, _hurtAudioVolume, _hurtAudioPitch);
    }
}
