using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SceneManager.LoadScene("nivell1");
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SceneManager.LoadScene("nivell2");
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SceneManager.LoadScene("nivell3");
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SceneManager.LoadScene("nivell4");
        if (Input.GetKeyDown(KeyCode.Alpha5))
            SceneManager.LoadScene("nivell5");
    }
}
