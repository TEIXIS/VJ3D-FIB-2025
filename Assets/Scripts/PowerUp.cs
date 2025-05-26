using UnityEngine;

public enum PowerUpType
{
    Magnet,
    ExtraBall,
    PowerBall,
    NormalBall
    // En el futuro: MultiBall, SpeedBall, etc.
}

[RequireComponent(typeof(Collider))]
public class PowerUp : MonoBehaviour
{
    [Tooltip("Tipo de power-up")]  
    public PowerUpType type;
    [Tooltip("Velocidad de caída del power-up")]
    public float fallSpeed = 2f;
    [Tooltip("Altura en Z para despawn si no se recoge")]
    public float despawnZ = -1f;

    void Update()
    {
        // Desplaza el power-up hacia la zona de la paleta (eje Z negativo)
        transform.position += Vector3.forward * fallSpeed * Time.deltaTime;

        // Si baja demasiado sin recogerse, destrúyelo
        if (transform.position.z < despawnZ)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Paddle"))
            return;

        // Encuentra la bola y aplica el efecto según el tipo
        BallController ball = Object.FindAnyObjectByType<BallController>();
        if (ball != null)
        {
            switch (type)
            {
                case PowerUpType.Magnet:
                    ball.ActivateMagnet();
                    Debug.Log("PowerUp: Magnet aplicado a la bola.");
                    break;
                case PowerUpType.ExtraBall:
                    ball.SpawnExtraBalls(1);
                    Debug.Log("PowerUp: Magnet aplicado a la bola.");
                    break;
                case PowerUpType.PowerBall:
                    ball.powerBallActive = true;
                    break;
                case PowerUpType.NormalBall:
                    ball.powerBallActive = false;
                    break;
                // Añade más casos aquí cuando tengas más tipos
            }
        }

        // Destruye el objeto del power-up tras la recogida
        Destroy(gameObject);
    }
}
