using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameOver = false;
    SpawnManager _spawnManager;
    UIManager _UIManager;
    public bool IsPaused { get; private set; } = false;

    private void Start()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        _UIManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (Input.GetButton("Restart") && _gameOver == true)
        {
            SceneManager.LoadScene(1); // 1 is the game scene now
        }

        if (Input.GetButtonDown("Pause") && _gameOver == false)
        {
            TogglePause();
        }

#if UNITY_STANDALONE
        // NEW: if player presses exit button (escape or back), exit the game
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }
#endif
    }

    void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0 : 1;
        _UIManager.TogglePause(IsPaused);
    }

    public void OnGameOver()
    {
        _gameOver = true;
        _spawnManager.OnGameOver();
    }
}
