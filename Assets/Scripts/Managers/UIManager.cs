using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField]
    private Text _scoreText;
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

    [Header("Game Over")]
    [SerializeField]
    private Text _gameOverText;
    [SerializeField]
    private Text _restartText;
    [SerializeField]
    private float _textFlashDelay;

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

    private int _totalScore;
    private GameManager _gameManager;

    void Start()
    {
        _totalScore = 0;
        AddScore(0);

        _gameOverText.gameObject.SetActive(false);
        _restartText.gameObject.SetActive(false);
        _waveText.gameObject.SetActive(false);

        _gameManager = FindObjectOfType<GameManager>();
        if (!_gameManager)
            Debug.LogError("UI Manager couldn't find Game Manager");

        ResetHeatGauge();
    }

    public void AddScore(int morePoints)
    {
        _totalScore += morePoints;
        _scoreText.text = $"Score: {_totalScore}";
    }

    public void OnStartNewWave(int waveNumber)
    {
        _waveText.text = $"Wave {waveNumber}";
        StartCoroutine(NewWaveRoutine());
    }

    public IEnumerator NewWaveRoutine()
    {
        Color zeroAlpha = new Color(_waveTextBaseColor.r, _waveTextBaseColor.g, _waveTextBaseColor.b, 0);
        float timeElapsed = 0;
        _waveText.gameObject.SetActive(true);
        // fade in
        while (timeElapsed < _waveColorBlendTime)
        {
            _waveText.color = Color.Lerp(zeroAlpha, _waveTextBaseColor, timeElapsed / _waveColorBlendTime);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        timeElapsed = 0;
        // shift between base and second color repeatedly
        while (timeElapsed < _waveTextDuration)
        {
            // alternate between 0 and 1 by using sin squared
            float lerpProgress = Mathf.Pow(Mathf.Sin(timeElapsed * (Mathf.PI /2) / _waveColorBlendTime), 2);
            _waveText.color = Color.Lerp(_waveTextBaseColor, _waveTextSecondColor, lerpProgress);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        Color fadeOutStart = _waveText.color;
        zeroAlpha = new Color(fadeOutStart.r, fadeOutStart.g, fadeOutStart.b, 0);
        timeElapsed = 0;
        //fade out
        while (timeElapsed < _waveColorBlendTime)
        {
            // alternate between 0 and 1 by using sin squared
            _waveText.color = Color.Lerp(fadeOutStart, zeroAlpha, timeElapsed / _waveColorBlendTime);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        _waveText.gameObject.SetActive(false);
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

    
}