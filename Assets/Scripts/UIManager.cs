using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private Image _LivesImg;
    [SerializeField]
    private Sprite[] _livesSprites;
    [SerializeField]
    private Text _gameOverText;
    [SerializeField]
    private Text _restartText;
    [SerializeField]
    private float _textFlashDelay;

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
        UpdateText();

        _gameOverText.gameObject.SetActive(false);
        _restartText.gameObject.SetActive(false);

        _gameManager = FindObjectOfType<GameManager>();
        if (!_gameManager)
            Debug.LogError("UI Manager couldn't find Game Manager");

        ResetHeatGauge();
    }

    public void AddScore(int morePoints)
    {
        _totalScore += morePoints;
        UpdateText();
    }

    private void UpdateText()
    {
        _scoreText.text = $"Score: {_totalScore}";
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

        while(_textFlashDelay > 0.01f) // To avoid infinite loops
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
}