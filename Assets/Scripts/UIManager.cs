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

    private int _totalScore;

    void Start()
    {
        _totalScore = 0;
        UpdateText();
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
    }
}