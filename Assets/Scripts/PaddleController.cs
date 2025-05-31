using UnityEngine;
using UnityEngine.SceneManagement;

public class PaddleController : MonoBehaviour
{
    public float speed = 10f;
    public float limitX = 5f;

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        Vector3 pos = transform.position;
        float h = 0f;
        if (sceneName == "nivell1" || sceneName == "nivell2")
        {
            // Movimiento horizontal (izquierda/derecha)
            h = Input.GetAxis("Horizontal") * -1;   
        }
        else if (sceneName == "nivell3" || sceneName == "nivell4" || sceneName == "nivell5")
        {
            h = Input.GetAxis("Vertical") * -1;
        }
        pos.x += h * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -limitX, +limitX);
        transform.position = pos;
    }
}
