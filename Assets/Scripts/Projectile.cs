using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Avanza hacia delante (eje Z local)
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("cub"))
        {
            Destroy(col.gameObject);  // destruye el bloque
            Destroy(gameObject);
            LevelManager lm = FindObjectOfType<LevelManager>();
            bool spawnedNext = false;
            if (lm != null)
            {
                spawnedNext = lm.BlockDestroyed(col.transform.position);
            }
            else
            {
                Debug.LogWarning("BallController: no se encontró LevelManager.");
            }      // destruye el proyectil
        }
        else if (col.gameObject.CompareTag("Grass"))
        {
            Destroy(col.gameObject);
            LevelManager lm = FindObjectOfType<LevelManager>();
            bool spawnedNext = false;
            if (lm != null)
            {
                spawnedNext = lm.BlockDestroyed(col.transform.position);
            }
            else
            {
                Debug.LogWarning("BallController: no se encontró LevelManager.");
            }
        }
        else if (!col.gameObject.CompareTag("Paddle"))
        {
            Destroy(gameObject);      // destruye al chocar con paredes, etc.
        }
    }
}
