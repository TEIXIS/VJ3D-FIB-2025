using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;
    private string currentScene;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        currentScene = SceneManager.GetActiveScene().name;

        // Solo reproducimos la música si estamos en menú o créditos
        if (IsMenuScene(currentScene) || IsCreditsScene(currentScene))
        {
            audioSource.Play();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;

        if (IsLevelScene(currentScene))
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
        else if ((IsMenuScene(currentScene) || IsCreditsScene(currentScene)) && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("nivell");
    }

    bool IsMenuScene(string sceneName)
    {
        return sceneName == "menu";
    }

    bool IsCreditsScene(string sceneName)
    {
        return sceneName == "credits";
    }
}
