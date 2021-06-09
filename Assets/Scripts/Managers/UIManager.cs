using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField]
    private Text _scoreText;
    private int _totalScore;
    [SerializeField]
    private Image _LivesImg;
    [SerializeField]
    private Sprite[] _livesSprites;

    [Header("Ammo")]
    [SerializeField]
    private Text _ammoText;
    [SerializeField]
    private string _ammoString = "Ammo:";
    private bool _ammoEmpty;
    [SerializeField]
    private GameObject[] _missileImages;
    [SerializeField]
    private Slider _upgradeGauge;
    [SerializeField]
    private string _upgradeString = "Upgrading...";

    [Header("Waves")]
    [SerializeField]
    private Text _waveText;
    [SerializeField]
    private Color _waveTextBaseColor;
    [SerializeField]
    private Color _waveTextSecondColor;
    [SerializeField]
    private float _waveColorBlendTime;
    [SerializeField]
    private float _waveTextDuration;

    [Header("Boss Wave")]
    [SerializeField]
    private Text _bossWaveText;
    [SerializeField]
    private Color _bossTextBaseColor;
    [SerializeField]
    private Color _bossTextSecondColor;
    [SerializeField]
    private float _bossColorBlendTime;
    [SerializeField]
    private float _bossTextDuration;
    [SerializeField]
    private Slider _bossHealthBar;
    [SerializeField]
    private Image _bossHealthBackground;
    [SerializeField]
    private Image _bossHealthFill;
    [SerializeField]
    private float _bossHealthFadeInTime = 3f;

    [Header("Game Over")]
    [SerializeField]
    private Text _gameOverText;
    [SerializeField]
    private Text _restartText;
    [SerializeField]
    private float _textFlashDelay;
    private GameManager _gameManager;

    [Header("Thruster Heat")]
    [SerializeField]
    private Slider _heatGauge;
    [SerializeField]
    private Image _heatFillImage;
    [SerializeField]
    private Color _overheatOriginalColor;
    [SerializeField]
    private Color _overHeatFlashColor;
    [SerializeField]
    private Text _heatText;
    [SerializeField]
    private string _overheatString = "Overheating!";
    [SerializeField]
    private string _normalHeatString = "Thruster Heat";
    private bool _overheatRoutine = false;

    [Header("Magnet")]
    [SerializeField]
    private Slider _magnetGauge;
    [SerializeField]
    private Image _magnetFillImage;
    [SerializeField]
    private Color _magnetOriginalColor;
    [SerializeField]
    private Color _magnetFlashColor;
    [SerializeField]
    private float _magnetColorBlendTime;
    [SerializeField]
    private Text _magnetText;
    [SerializeField]
    private string _magnetRecharingString = "Recharging";
    [SerializeField]
    private string _magnetString = "Magnet";
    [SerializeField]
    private string _magnetUseString = "Magnetizing!";
    private bool _magnetFlashRoutine;

    void Start()
    {
        _totalScore = 0;
        AddScore(0);

        _gameOverText.gameObject.SetActive(false);
        _restartText.gameObject.SetActive(false);
        _waveText.gameObject.SetActive(false);
        _bossWaveText.gameObject.SetActive(false);
        _bossHealthBar.gameObject.SetActive(false);

        _gameManager = FindObjectOfType<GameManager>();
        if (!_gameManager)
            Debug.LogError("UI Manager couldn't find Game Manager");

        ResetHeatGauge();
        UpdateMagnetRecharge(1f);
    }

    public void AddScore(int morePoints)
    {
        _totalScore += morePoints;
        _scoreText.text = $"Score: {_totalScore}";
    }

    public void OnStartNewWave(int waveNumber)
    {
        _waveText.text = $"Wave {waveNumber}";
        StartCoroutine(NewWaveRoutine(
            _waveText, _waveTextBaseColor, _waveTextSecondColor, _waveColorBlendTime, _waveTextDuration));
    }

    IEnumerator NewWaveRoutine(Text textObject, Color defaultColor, Color altColor, float blendTime, float duration)
    {
        Color zeroAlpha = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0);
        float timeElapsed = 0;
        textObject.gameObject.SetActive(true);
        // fade in
        while (timeElapsed < blendTime)
        {
            textObject.color = Color.Lerp(zeroAlpha, defaultColor, timeElapsed / blendTime);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        timeElapsed = 0;
        // shift between base and second color repeatedly
        while (timeElapsed < duration)
        {
            // alternate between 0 and 1 by using sin squared
            float lerpProgress = Mathf.Pow(Mathf.Sin(timeElapsed * (Mathf.PI /2) / blendTime), 2);
            textObject.color = Color.Lerp(defaultColor, altColor, lerpProgress);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        Color fadeOutStart = textObject.color;
        zeroAlpha = new Color(fadeOutStart.r, fadeOutStart.g, fadeOutStart.b, 0);
        timeElapsed = 0;
        //fade out
        while (timeElapsed < blendTime)
        {
            textObject.color = Color.Lerp(fadeOutStart, zeroAlpha, timeElapsed / blendTime);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        textObject.gameObject.SetActive(false);
    }

    public void UpdateLives(int currentLives)
    {
        _LivesImg.sprite = _livesSprites[currentLives];

        if (currentLives <= 0)
            StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        _gameManager.OnGameOver();
        _gameOverText.gameObject.SetActive(true);
        _restartText.gameObject.SetActive(true);
        WaitForSeconds flashDelay = new WaitForSeconds(_textFlashDelay);

        while(_textFlashDelay > 0.05f) // To avoid infinite loops
        {
            yield return flashDelay;
            _gameOverText.color = Color.red;
            yield return flashDelay;
            _gameOverText.color = Color.white;
        }
    }

    public void UpdateHeatGauge(float gaugeAmount)
    {
        _heatGauge.value = gaugeAmount;

        //Overheat when gauge is full
        if (gaugeAmount >= 1)
        {
            _heatText.text = _overheatString;
            _overheatRoutine = true;
            StartCoroutine(OverheatingRoutine());
        }

        //Stop overheating when gauge is empty
        if (gaugeAmount <= 0 && _overheatRoutine)
        {
            ResetHeatGauge();
        }
    }

    void ResetHeatGauge()
    {
        _heatText.text = _normalHeatString;
        _overheatRoutine = false;
    }

    IEnumerator OverheatingRoutine()
    {
        WaitForSeconds flashTime = new WaitForSeconds(0.3f);
        while (_overheatRoutine)
        {
            _heatFillImage.color = _overHeatFlashColor;
            yield return flashTime;
            _heatFillImage.color = _overheatOriginalColor;
            yield return flashTime;
        }       
    }

    public void UpdateAmmoCount(int ammoCount, int ammoMaximum)
    {
        _upgradeGauge.gameObject.SetActive(false);
        _ammoText.text = $"{_ammoString} {ammoCount} / {ammoMaximum}";

        if (ammoCount <= 0)
        {
            _ammoEmpty = true;
            StartCoroutine(AmmoEmptyRoutine());
            return;
        }

        _ammoEmpty = false;
    }

    public void UpdateWeaponUpgrade(float progress)
    {
        _upgradeGauge.gameObject.SetActive(true);
        _ammoText.text = _upgradeString;
        _upgradeGauge.value = progress;
    }

    IEnumerator AmmoEmptyRoutine() // causes ammo text to flash red
    {
        WaitForSeconds flashDelay = new WaitForSeconds(_textFlashDelay);
        while (_ammoEmpty && _textFlashDelay >= 0.05f)
        {
            _ammoText.color = Color.red;
            yield return flashDelay;
            _ammoText.color = Color.white;
            yield return flashDelay;
        }
    }

    public void UpdateMissileCount(int missileCount)
    {
        for (int i = 0; i < _missileImages.Length; i++)
        {
            // if missileCount is 5, sets images 0-4 active
            // etc
            _missileImages[i].SetActive(i < missileCount);
        }
    }

    public void OnMagnetUsed()
    {
        _magnetText.text = _magnetUseString;
        StartCoroutine(MagnetFlashingRoutine());
    }

    IEnumerator MagnetFlashingRoutine()
    {
        _magnetFlashRoutine = true;
        float timeElapsed = 0;
        while (_magnetFlashRoutine)
        {                     
            // alternate between 0 and 1 by using sin squared
            float lerpProgress = Mathf.Pow(Mathf.Sin(timeElapsed * (Mathf.PI / 2) / _magnetColorBlendTime), 2);
            _magnetFillImage.color = Color.Lerp(_magnetOriginalColor, _magnetFlashColor, lerpProgress);
            yield return null;
            timeElapsed += Time.deltaTime;
        }
        _magnetFillImage.color = _magnetOriginalColor;
    }

    public void UpdateMagnetRecharge(float progress)
    {
        _magnetFlashRoutine = false;
        _magnetGauge.value = progress;
        _magnetText.text = (progress >= 1f) ? _magnetString : _magnetRecharingString;
    }

    public void OnBossWave()
    {
        StartCoroutine(NewWaveRoutine(
            _bossWaveText, _bossTextBaseColor, _bossTextSecondColor, _bossColorBlendTime, _bossTextDuration));
    }

    public void OnBossAppear()
    {
        StartCoroutine(BossHealthFadeInRoutine());
    }

    IEnumerator BossHealthFadeInRoutine()
    {
        _bossHealthBar.gameObject.SetActive(true);
        Color backgroundColor = _bossHealthBackground.color;
        Color fillColor = _bossHealthFill.color;
        Color zeroAlphaBackground = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0);
        Color zeroAlphaFill = new Color(fillColor.r, fillColor.g, fillColor.b, 0);
        float timeElapsed = 0;
        // fade in
        while (timeElapsed < _bossHealthFadeInTime)
        {
            timeElapsed += Time.deltaTime;
            float progress = timeElapsed / _bossHealthFadeInTime;
            _bossHealthBackground.color = Color.Lerp(zeroAlphaBackground, backgroundColor, progress);
            _bossHealthFill.color = Color.Lerp(zeroAlphaFill, fillColor, progress);
            yield return null;           
        }
    }

    public void UpdateBossLife(float lifeRemaining)
    {
    }
}