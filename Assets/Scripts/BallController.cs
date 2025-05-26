using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Tooltip("Velocidad constante de la bola")]  
    public float speed = 10f;
    public GameObject ballPrefab;
    // Prefab del power-up imán
    public GameObject magnetPrefab;
    public GameObject extraBallPrefab;

    public GameObject powerBallPrefab;

    public GameObject normalBallPrefab;

    private Rigidbody rb;
    private bool launched = false;
    private float radius;

    private GameObject paddle;
    private Vector3 lastPaddlePos;
    private bool paddleControlActive = false;

    public bool powerBallActive = false;

    // IMÁN
    private bool magnetQueued = false;    // se activará en el siguiente rebote en paleta
    private bool magnetActive = false;    // bola pegada a la paleta
    private Vector3 stickOffset;
     public float minZComponent = 0.2f;

        private Vector3 ClampDirection(Vector3 dir)
    {
        dir.y = 0;
        dir.Normalize();

        if (Mathf.Abs(dir.z) < minZComponent)
        {
            float signZ = dir.z >= 0 ? 1f : -1f;
            dir.z = minZComponent * signZ;
            // recalculamos x para que x² + z² = 1 (y=0)
            float xSign = dir.x >= 0 ? 1f : -1f;
            dir.x = Mathf.Sqrt(1 - dir.z * dir.z) * xSign;
        }

        return dir;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configuración física
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY;

        radius = transform.localScale.x * 0.5f;

        paddle = GameObject.FindWithTag("Paddle");
        lastPaddlePos = paddle.transform.position;
    }

    void Update()
    {
        // Si bola está pegada por imán, sigue a la paleta y espera Space
        if (magnetActive && !launched)
        {
            transform.position = paddle.transform.position + stickOffset;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                magnetActive = false;
                LaunchInRandomDirection();
            }
            return;
        }

        // Lanzamiento inicial o normal si no ha sido lanzada
        if (!launched && Input.GetKeyDown(KeyCode.Space))
        {
            LaunchInInitialDirection();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            powerBallActive = !powerBallActive;
            Debug.Log("Power Ball " + (powerBallActive ? "activado" : "desactivado"));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            SpawnExtraBalls(2);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            paddleControlActive = !paddleControlActive;
            Debug.Log("Control de paleta " + (paddleControlActive ? "activado" : "desactivado"));
        }
    }

    void FixedUpdate()
    {
        if (!launched) return;

        // Forzar velocidad constante en XZ
        Vector3 v = rb.linearVelocity;
        v.y = 0;
        rb.linearVelocity = v.normalized * speed;

        // Giro sin deslizamiento
        if (v.sqrMagnitude > 0.001f)
        {
            Vector3 axis = Vector3.Cross(Vector3.forward, v.normalized);
            float omega = v.magnitude / radius;
            rb.angularVelocity = axis * omega;
        }

        // Influencia de movimiento de paleta
        if (paddleControlActive)
        {
            Vector3 paddleDelta = paddle.transform.position - lastPaddlePos;
            float influence = paddleDelta.x * 10f;
            Vector3 vec = rb.linearVelocity;
            vec.x += influence;
            rb.linearVelocity = vec.normalized * speed;
            lastPaddlePos = paddle.transform.position;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // Rebote en paleta
        if (col.gameObject.CompareTag("Paddle"))
        {
            if (magnetQueued)
            {
                magnetQueued = false;
                magnetActive = true;
                launched = false;
                rb.isKinematic = true;
                stickOffset = transform.position - paddle.transform.position;
                Debug.Log("Imán: bola pegada a la paleta");
                return;
            }
            if (magnetActive)
                return;
            float hitX = transform.position.x - col.transform.position.x;
            float halfW = col.collider.bounds.size.x / 2f;
            float nx = Mathf.Clamp(hitX / halfW, -1f, 1f);
            Vector3 dir = new Vector3(nx, 0f, -1f).normalized;
            rb.linearVelocity = dir * speed;
            return;
        }

        // Colisión con cubo
        if (col.gameObject.CompareTag("cub"))
        {
            ContactPoint cp = col.GetContact(0);
            Vector3 incoming = rb.linearVelocity;
            Vector3 normal = cp.normal.normalized;

            // Guardamos posición antes de destruir
            Vector3 spawnPos = col.transform.position;
            Destroy(col.gameObject);

            Object.FindAnyObjectByType<ScoreManager>()?.AddPoints(100);

            // Probabilidad de soltar imán (ej. 30%)
            if (magnetPrefab != null && Random.value < 0.3f)
            {
                Instantiate(magnetPrefab, spawnPos, Quaternion.identity);
                Debug.Log("Power-Up Imán generado en " + spawnPos);
            }
            else if (extraBallPrefab != null && Random.value < 0.3f)
            {
                Instantiate(extraBallPrefab, spawnPos, Quaternion.identity);
                Debug.Log("BallExtra generado en " + spawnPos);
            }
            else if (powerBallPrefab != null && Random.value < 0.3f)
            {
                Instantiate(powerBallPrefab, spawnPos, Quaternion.identity);
                Debug.Log("Haunter generado en " + spawnPos);
            }
            else if (powerBallActive && normalBallPrefab != null && Random.value < 0.9)
            {
                Instantiate(normalBallPrefab, spawnPos, Quaternion.identity);
            }

            Vector3 refl;

            if (!powerBallActive && Vector3.Dot(incoming, normal) < 0f)
            {
                refl = Vector3.Reflect(incoming, normal);
            }
            else
            {
                refl = incoming;
            }

            // ajustamos la dirección para garantizar minZ
            Vector3 dirClamped = ClampDirection(refl);
            rb.linearVelocity = dirClamped * speed;
            return;
        }

        // Otras colisiones
        ContactPoint cp2 = col.GetContact(0);
        Vector3 in2   = rb.linearVelocity;
        Vector3 norm2 = cp2.normal.normalized;
        if (Vector3.Dot(in2, norm2) < 0f)
        {
            Vector3 refl2 = Vector3.Reflect(in2, norm2);
            Vector3 dir2  = ClampDirection(refl2);
            rb.linearVelocity = dir2 * speed;
        }
    }

    public void SpawnExtraBalls(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            GameObject newBall = Instantiate(ballPrefab, transform.position + offset, Quaternion.identity);
            BallController ballScript = newBall.GetComponent<BallController>();
            ballScript.LaunchInRandomDirection();
        }
    }

    private void LaunchInInitialDirection()
    {
        launched = true;
        rb.isKinematic = false;
        Vector3 dir = new Vector3(1, 0, -1).normalized;
        rb.linearVelocity = dir * speed;
        Debug.Log("Bola lanzada: dirección inicial");
    }

    public void LaunchInRandomDirection()
    {
        if (launched) return;
        launched = true;
        rb.isKinematic = false;
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, -1f).normalized;
        rb.linearVelocity = dir * speed;
        Debug.Log("Bola lanzada: dirección aleatoria " + dir);
    }

    public void ActivateMagnet()
    {
        magnetQueued = true;
        Debug.Log("Power-Up Imán recogido: la próxima vez que toque la paleta se quedará pegada");
    }
}
