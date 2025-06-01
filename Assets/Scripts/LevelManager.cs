using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Prefab del PowerUp de Siguiente Nivel")]
    public GameObject nextLevelPowerUpPrefab;
    [Header("Porcentaje de bloques a destruir para generar el PowerUp")]
    [Range(0.0f, 1.0f)]
    public float threshold = 0.95f;

    private int totalBlocks;
    private int destroyedBlocks;
    private bool powerUpSpawned = false;

    void Start()
    {
        // Contamos cuántos bloques hay al inicio:
        GameObject[] bloques = GameObject.FindGameObjectsWithTag("cub");
        totalBlocks = bloques.Length;
        destroyedBlocks = 0;
        powerUpSpawned = false;
        if (totalBlocks == 0)
            Debug.LogWarning("LevelManager: no se encontraron bloques con tag 'cub'.");
    }

    /// <summary>
    /// Debe llamarse cada vez que se destruye un bloque. 
    /// Devuelve true si, justo en esta llamada, generó el power-up de next-level.
    /// </summary>
    public bool BlockDestroyed(Vector3 bloquePosition)
    {
        destroyedBlocks++;

        if (totalBlocks <= 0)
            return false;

        float porcentaje = (float)destroyedBlocks / totalBlocks;

        // Si llegamos al 95% Y aún no habíamos generado el next-level, lo instanciamos ahora:
        if (!powerUpSpawned && porcentaje >= threshold)
        {
            powerUpSpawned = true;
            SpawnNextLevelPowerUp(bloquePosition);
            return true;
        }

        return false;
    }

    private void SpawnNextLevelPowerUp(Vector3 bloquePosition)
    {
        if (nextLevelPowerUpPrefab == null)
        {
            Debug.LogError("LevelManager: faltó asignar nextLevelPowerUpPrefab.");
            return;
        }

        // Calcula índice de la escena actual y el siguiente
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("LevelManager: último nivel, no hay siguiente.");
            return;
        }

        Vector3 spawnPos = bloquePosition + Vector3.up * 0.5f;
        GameObject instancia = Instantiate(nextLevelPowerUpPrefab, spawnPos, Quaternion.identity);

        NextLevelPowerUp nextScript = instancia.GetComponent<NextLevelPowerUp>();
        if (nextScript != null)
        {
            nextScript.nextSceneBuildIndex = nextIndex;
        }
        else
        {
            Debug.LogError("LevelManager: prefab no tiene NextLevelPowerUp.cs.");
        }

        Debug.Log($"LevelManager: 95% de bloques destruidos → generé power-up nivel {nextIndex}.");
    }
}
