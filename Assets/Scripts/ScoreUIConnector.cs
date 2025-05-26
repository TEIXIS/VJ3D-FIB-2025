using UnityEngine;
using TMPro;

public class ScoreUIConnector : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.scoreText = scoreText;
            ScoreManager.Instance.highScoreText = highScoreText;
            ScoreManager.Instance.UpdateUI(); // para refrescar la interfaz
        }
    }
}
