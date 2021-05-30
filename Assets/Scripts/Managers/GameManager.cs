using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameOver = false;

    void Update()
    {
        if (Input.GetButton("Restart") && _gameOver == true)
        {
            SceneManager.LoadScene(1); // 1 is the game scene now
        }

        #if UNITY_STANDALONE
        // NEW: if player presses exit button (escape or back), exit the game
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }
        #endif
    }

    public void OnGameOver()
    {
        _gameOver = true;
    }
}
