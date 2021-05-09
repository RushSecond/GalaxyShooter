using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameOver = false;

    void Update()
    {
        // if player presses restart button and the game is over, reload the scene
        if (Input.GetButton("Restart") && _gameOver == true)
        {
            SceneManager.LoadScene(0); // 0 is the game scene
        }
    }

    public void GameOver()
    {
        _gameOver = true;
    }
}
