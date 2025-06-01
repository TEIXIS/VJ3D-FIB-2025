using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int currentScore = 0;
    public int highScore = 0;

    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        SceneManager.sceneLoaded += OnSceneLoaded;
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

    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Punts: " + currentScore;

        if (highScoreText != null)
            highScoreText.text = "HighScore: " + highScore;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "menu")
    {
        ResetScore(); // ← aquí reinicia puntos al volver al menú
        UpdateUI();   // ← opcional, para refrescar el texto
    }
}

}
