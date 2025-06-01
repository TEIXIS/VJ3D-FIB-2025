using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class NextLevelPowerUp : MonoBehaviour
{
    [Tooltip("Índice en Build Settings de la siguiente escena. Si es < 0, se ignorará.")]
    public int nextSceneBuildIndex = -1;
    public float moveSpeed = 2f;
    void Update()
    {
        // Mueve el GameObject en +Z a velocidad constante
        if(transform.position.z <= -2.674) transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Asumimos que la paleta (paddle) tiene tag = "Paddle"
        if (!other.CompareTag("Paddle")) return;

        Debug.Log("NextLevelPowerUp: recogido. Intentando cargar siguiente nivel…");

        if (nextSceneBuildIndex >= 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
        else
        {
            Debug.LogError($"NextLevelPowerUp: nextSceneBuildIndex inválido ({nextSceneBuildIndex}).");
        }

        Destroy(gameObject);
    }
}
