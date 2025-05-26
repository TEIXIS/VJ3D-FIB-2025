using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ScoreManager : MonoBehaviour
{
    public int currentScore = 0;
    public int highScore = 0;

    public TMP_Text scoreText;
    public TMP_Text highScoreText;


    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        currentScore += amount;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Punts: " + currentScore;

        if (highScoreText != null)
            highScoreText.text = "HighScore: " + highScore;
    }
}
