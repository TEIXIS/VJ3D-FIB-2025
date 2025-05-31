using UnityEngine;
using UnityEngine.SceneManagement;

public class PaddleController : MonoBehaviour
{
    [Header("Velocidad y límites de la paleta")]
    public float speed = 10f;
    public float limitX = 5f;

    // Referencia al Animator del Pokémon (puede cambiar en tiempo de ejecución)
    private Animator pokeAnimator;

    void Update()
    {
        // Si no tenemos referencia o el Animator actual se ha destruido, búscalo de nuevo:
        if (pokeAnimator == null)
        {
            pokeAnimator = GetComponentInChildren<Animator>(includeInactive: false);
            if (pokeAnimator == null)
            {
                // A veces estar en el mismo frame que se instanció Timburr / Conkeldurr el Animator
                // todavía no está habilitado, así que no imprimimos advertencia a cada frame.
                // Sólo una vez, la primera vez que sea null y no haya nada que encontrar.
                Debug.LogWarning("PaddleController: no se encontró ningún Animator en los hijos.");
            }
        }

        // Ahora movemos la paleta normalmente...
        float h = Input.GetAxis("Horizontal") * -1f;

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

        // Y actualizamos isMoving en el Animator si lo encontramos
        if (pokeAnimator != null)
        {
            bool moving = !Mathf.Approximately(h, 0f);
            pokeAnimator.SetBool("isWalking", moving);
        }
    }
}
