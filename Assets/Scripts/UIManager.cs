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
    private float _textFlashDelay;

    private int _totalScore;

    void Start()
    {
        _totalScore = 0;
        UpdateText();

        // make sure game over text is turned off
        _gameOverText.gameObject.SetActive(false);
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

        // if lives are zero, activate the game over text
        if (currentLives <= 0)   
            StartCoroutine(GameOverFlashingRoutine());
    }

    IEnumerator GameOverFlashingRoutine()
    {
        _gameOverText.gameObject.SetActive(true);
        WaitForSeconds flashDelay = new WaitForSeconds(_textFlashDelay);

        while(_textFlashDelay > 0.01f) // To avoid infinite loops
        {
            yield return flashDelay;
            _gameOverText.color = Color.red;
            yield return flashDelay;
            _gameOverText.color = Color.white;
        }
    }
}