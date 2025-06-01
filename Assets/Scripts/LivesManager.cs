using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;


public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    [Header("Configuración")]
    public int initialLives = 3;
    public GameObject ballPrefab;   // Asigna aquí el prefab de la bola
    public Transform paddle;        // Arrastra la paleta desde la escena

    public GameObject gameOverText;


    [HideInInspector] public int livesLeft;
    [HideInInspector] public bool waitingForLaunch = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        livesLeft = initialLives;
    }

    // Llamado desde BallController cuando una bola sale del campo
    public void OnBallLost(BallController lostBall)
    {
        Destroy(lostBall.gameObject);


        BallController[] allBalls = FindObjectsOfType<BallController>();
        if (allBalls.Length == 1 && allBalls[0] == lostBall)
        {
            livesLeft--;
            Debug.Log("Restar vida");
        }
        // ¿Quedan otras bolas activas?
        else
        {
            Debug.Log("Aún hay bolas activas");
            return;   // Aún hay bolas, no resta vida
        }

        if (livesLeft > 0)
        {
            SpawnNewBall();
        }
        else
        {
            if (gameOverText != null)
                gameOverText.SetActive(true);

            StartCoroutine(BackToMenuAfterDelay());
        }

    }

    void SpawnNewBall()
    {
        // Usa la posición inicial global que guardó la primera bola
        Vector3 pos = BallController.InitialSpawnPosition;
        GameObject newBall = Instantiate(ballPrefab, pos, Quaternion.identity);
        Debug.Log("Nueva bola generada");

        waitingForLaunch = true;   // Se lanzará cuando el jugador pulse Espacio
        Debug.Log($"Vida perdida. Vidas restantes = {livesLeft}");
    }

    private IEnumerator BackToMenuAfterDelay()
    {
        SceneManager.LoadScene("menu");

        if (gameOverText != null)
            gameOverText.SetActive(true);

        yield return new WaitForSeconds(3f);

        Debug.Log("Han pasat 3 segundos, volvemos al menú");
        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
            Debug.Log("Desactivamos el texto de Game Over");
        }              

        
    }


}
