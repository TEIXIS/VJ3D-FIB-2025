using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public GameObject telekinesisPrefab;

    public GameObject emberPrefab;

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


    public bool attractionActive = false;
    public float attractionStrength = 5f;

    public GameObject projectilePrefab;
    public GameObject charizardPrefab;

    public float projectileHeight = 10.502f; // altura del proyectil respecto al Charizard
    public float shootInterval = 2f;
    public float shootDuration = 6f;
    public float charizardOffset = 0.3f;   // separación en X respecto al halfWidth
    public float charizardHeight = 0.5f;   // elevación sobre la paleta

    // Variables internas
    private bool shooterActive = false;
    private GameObject leftChar, rightChar;

    public GameObject slowpokePrefab;
    public float slowDuration = 8f;
    public float slowFactor = 0.5f;  // 50 % de velocidad
    private bool slowActive = false;


    public GameObject timburrPrefab;
    public GameObject conkeldurrPrefab;

    public AudioClip breakSound;
    public AudioClip grassSound;
    public AudioClip bounceSound;

    public GameObject breakParticlesPrefab; 

    public static Vector3 InitialSpawnPosition;

    private AudioSource audioSource;

    // Ejemplo de pesos, ajústalos a tu gusto
    private Dictionary<GameObject, float> powerUpWeights;


    private Vector3 ClampDirection(Vector3 dir)
    {
        dir.y = 0;
        dir.Normalize();

        if (Mathf.Abs(dir.z) < minZComponent)
        {
            Debug.Log("ClampDirection: dir.z %f es menor que minZComponent %f, ajustando dirección" + dir.z + minZComponent);
            float signZ = dir.z >= 0 ? 1f : -1f;
            dir.z = minZComponent * signZ;
            // recalculamos x para que x² + z² = 1 (y=0)
            float xSign = dir.x >= 0 ? 1f : -1f;
            dir.x = Mathf.Sqrt(1 - dir.z * dir.z) * xSign;
        }

        return dir;
    }

    void Start()
    {
        // Inicializamos el diccionario de pesos una sola vez
        powerUpWeights = new Dictionary<GameObject, float>()
        {
            { magnetPrefab,        7f },
            { timburrPrefab,        4f },
            { conkeldurrPrefab,     4f },
            { slowpokePrefab,       3f },
            { emberPrefab,          3f },
            { extraBallPrefab,     5f },
            { telekinesisPrefab,    3f },
            { powerBallPrefab,      4f }
        };
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

        audioSource = GetComponent<AudioSource>();

        if (InitialSpawnPosition == Vector3.zero)
            InitialSpawnPosition = transform.position;

    }

    void Update()
    {
        // Si estamos esperando lanzamiento después de perder vida
        if (LivesManager.Instance != null &&
            LivesManager.Instance.waitingForLaunch &&
            !launched)
        {
            // Fija la bola en la posición de spawn hasta que se pulse Espacio
            transform.position = InitialSpawnPosition;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                LivesManager.Instance.waitingForLaunch = false;
                LaunchInInitialDirection();
            }
            return; // No procesar nada más hasta que se lance
        }


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
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnExtraBalls(2);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            paddleControlActive = !paddleControlActive;
            Debug.Log("Control de paleta " + (paddleControlActive ? "activado" : "desactivado"));
        }

        // --- COMPROBAR SI SE HA PERDIDO LA BOLA ---
        float outZ = paddle.transform.position.z + 1.0f;  // Ajusta margen si lo necesitas
        if (transform.position.z > outZ)  // Sale por detrás de la paleta
        {
            if (LivesManager.Instance != null)
                LivesManager.Instance.OnBallLost(this);
        }

    }

    void FixedUpdate()
    {
        if (!launched) return;

        // Forzar velocidad constante en XZ
        Vector3 v = rb.linearVelocity;
        v.y = 0;
        rb.linearVelocity = v.normalized * speed;

        if (attractionActive && v.z > 0f && paddle != null)
        {
            // 2.a) Calcula extents de la paleta
            var col = paddle.GetComponentInChildren<Collider>();
            float halfW = col.bounds.extents.x;

            // 2.b) Distancia horizontal
            float dx = paddle.transform.position.x - transform.position.x;

            // 2.c) Solo si está fuera del rango X de la paleta
            if (Mathf.Abs(dx) > halfW)
            {
                // 2.d) Dirección deseada en X (+1 o –1)
                float dirX = Mathf.Sign(dx);

                // 3) Interpola la componente X hacia ±maxX
                //    maxX será < speed, tú decides cuánta inclinación en Z dejas
                float maxX = speed * 0.8f;
                v.x = Mathf.MoveTowards(v.x, dirX * maxX, attractionStrength * Time.fixedDeltaTime);

                // 4) Reajusta Z para mantener |v| = speed
                float zSign = Mathf.Sign(v.z) != 0f ? Mathf.Sign(v.z) : 1f;
                float zMag = Mathf.Sqrt(Mathf.Max(0f, speed * speed - v.x * v.x));
                v.z = zSign * zMag;

                Debug.Log($"[Attract] vx={v.x:0.00}, vz={v.z:0.00}");
            }
        }

        rb.linearVelocity = v;

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

            AudioSource.PlayClipAtPoint(bounceSound, transform.position);

            return;
        }


        // Colisión con cubo
        if (col.gameObject.CompareTag("cub"))
        {
            ContactPoint cp = col.GetContact(0);
            Vector3 incoming = rb.linearVelocity;
            Vector3 normal = cp.normal.normalized;

            // Guardamos posición antes de destruir


            Object.FindAnyObjectByType<ScoreManager>()?.AddPoints(100);

            if (breakParticlesPrefab != null)
            {
                GameObject psGO = Instantiate(breakParticlesPrefab, col.transform.position, Quaternion.identity);
                ParticleSystem ps = psGO.GetComponent<ParticleSystem>();

                Renderer rend = col.gameObject.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    Color blockColor = rend.material.color;
                    var main = ps.main;
                    main.startColor = blockColor;
                }
            }


            Vector3 spawnPos = col.transform.position;
            Destroy(col.gameObject);

            // 1) Notificamos al LevelManager y guardamos si generó el next-level:
            LevelManager lm = FindObjectOfType<LevelManager>();
            bool spawnedNext = false;
            if (lm != null)
            {
                spawnedNext = lm.BlockDestroyed(spawnPos);
            }
            else
            {
                Debug.LogWarning("BallController: no se encontró LevelManager.");
            }

            // 2) Solo si NO se generó el next-level, hacemos el spawn aleatorio de power-ups normales:
            if (!spawnedNext)
            {
                if (Random.value < 0.4) GenerarPowerUpPonderado(spawnPos);
            }
            // Probabilidad de soltar imán (ej. 30%)


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

            AudioSource.PlayClipAtPoint(breakSound, transform.position);

            return;
        }
        if (col.gameObject.CompareTag("Grass"))
        {
            //Ignora colisiones
            if (breakParticlesPrefab != null)
            {
                GameObject psGO = Instantiate(breakParticlesPrefab, col.transform.position, Quaternion.identity);
                ParticleSystem ps = psGO.GetComponent<ParticleSystem>();

                Renderer rend = col.gameObject.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    Color blockColor = rend.material.color;
                    var main = ps.main;
                    main.startColor = blockColor;
                }
            }

            LevelManager lm = FindObjectOfType<LevelManager>();
            if (lm != null)
            {
                lm.BlockDestroyed(col.transform.position);
            }
            else
            {
                Debug.LogWarning("BallController: no se encontró LevelManager.");
            }
            Destroy(col.gameObject);
            Vector3 incoming = rb.linearVelocity;
            Vector3 dirClamped = ClampDirection(incoming);
            rb.linearVelocity = dirClamped * speed;
            AudioSource.PlayClipAtPoint(grassSound, transform.position);
            return;
         }

        // Otras colisiones
        ContactPoint cp2 = col.GetContact(0);
        Vector3 in2 = rb.linearVelocity;
        Vector3 norm2 = cp2.normal.normalized;
        if (Vector3.Dot(in2, norm2) < 0f)
        {
            Vector3 refl2 = Vector3.Reflect(in2, norm2);
            Vector3 dir2 = ClampDirection(refl2);
            rb.linearVelocity = dir2 * speed;
        }
        if (col.gameObject.CompareTag("Wall"))
        {
            AudioSource.PlayClipAtPoint(bounceSound, transform.position);
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


    private IEnumerator AttractionCoroutine(float duration)
    {
        attractionActive = true;
        yield return new WaitForSeconds(duration);
        attractionActive = false;
    }
    public void ActivateAttraction(float duration)
    {
        if (attractionActive) return;
        StartCoroutine(AttractionCoroutine(duration));
    }


    private IEnumerator ShooterCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < shootDuration)
        {
            // Lanza proyectiles desde la posición actual de cada Charizard
            Vector3 pR = rightChar.transform.position;
            pR.y = projectileHeight;

            Vector3 pL = leftChar.transform.position;
            pL.y = projectileHeight;

            Instantiate(projectilePrefab,
                        leftChar.transform.position - Vector3.up * charizardHeight,
                        Quaternion.identity);
            Instantiate(projectilePrefab,
                        rightChar.transform.position - Vector3.up * charizardHeight,
                        Quaternion.identity);

            yield return new WaitForSeconds(shootInterval);
            elapsed += shootInterval;
        }

        // 4) Al acabar, destruye los Charizard y libera recursos
        Destroy(leftChar);
        Destroy(rightChar);
        shooterActive = false;
    }

    public void ActivateShooter()
    {
        if (shooterActive)
        {
            StopCoroutine("ShooterCoroutine");
            Destroy(leftChar);
            Destroy(rightChar);
            shooterActive = false;
        }

        shooterActive = true;

        // Calcula mitad de ancho de la paleta
        //Busco collider en hijos de la paddle


        Collider col = paddle.GetComponentInChildren<Collider>();
        float halfW = col.bounds.extents.x;

        // Posiciones relativas a la paleta
        Vector3 basePos = paddle.transform.position + Vector3.up * charizardHeight + Vector3.back * 0.2f;
        Vector3 leftPos = basePos + Vector3.left * (halfW + charizardOffset);
        Vector3 rightPos = basePos + Vector3.right * (halfW + charizardOffset);

        // Instanciamos y parentamos a la paleta
        leftChar = Instantiate(charizardPrefab, leftPos, Quaternion.LookRotation(Vector3.back));
        rightChar = Instantiate(charizardPrefab, rightPos, Quaternion.LookRotation(Vector3.back));

        leftChar.transform.SetParent(paddle.transform, worldPositionStays: true);
        rightChar.transform.SetParent(paddle.transform, worldPositionStays: true);

        StartCoroutine(ShooterCoroutine());
    }


    private IEnumerator SlowCoroutine()
    {
        slowActive = true;
        // Guardas la velocidad original de la bola
        float origSpeed = speed;
        speed *= slowFactor;

        // (Opcional) ralentiza todo el juego
        Time.timeScale = 0.7f;

        yield return new WaitForSecondsRealtime(slowDuration);

        // Vuelves a normal
        speed = origSpeed;
        Time.timeScale = 1f;
        slowActive = false;
    }

    public void ActivateSlow()
    {
        if (!slowActive)
        {
            StartCoroutine(SlowCoroutine());
        }
        else
        {
            // Si ya está activo, reinicia la duración:
            StopCoroutine("SlowCoroutine");
            StartCoroutine(SlowCoroutine());
        }
    }
    
    private void GenerarPowerUpPonderado(Vector3 spawnPos)
    {
        // 1) Crear un diccionario dinámico que incluya solo los power-ups válidos ahora
        Dictionary<GameObject, float> candidates = new Dictionary<GameObject, float>(powerUpWeights);

        // Si powerBallActive = true, ENABLE normalBallPrefab con peso alto; si no, lo excluimos.
        if (powerBallActive && normalBallPrefab != null)
        {
            // Peso alto (ej. 90) para que salga muy a menudo
            candidates.Add(normalBallPrefab, 90f);
        }

        // 2) Eliminar cualquier entrada cuyo prefab sea null (no asignado)
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kv in candidates)
        {
            if (kv.Key == null)
                toRemove.Add(kv.Key);
        }
        foreach (var key in toRemove)
            candidates.Remove(key);

        // 3) Sumar todos los pesos
        float totalWeight = 0f;
        foreach (var kv in candidates)
            totalWeight += kv.Value;

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("GenerarPowerUpPonderado: no hay power-ups válidos (suma de pesos = 0).");
            return;
        }

        // 4) Elegir un valor aleatorio entre 0 y totalWeight
        float r = Random.Range(0f, totalWeight);

        // 5) Recorrer acumulando hasta que r ≤ acumulado
        float acum = 0f;
        foreach (var kv in candidates)
        {
            GameObject prefab = kv.Key;
            float weight = kv.Value;

            acum += weight;
                Quaternion rotacionAUsar = Quaternion.identity;

            if (prefab == magnetPrefab)
            {
                // El ejemplo que tenías antes:
                rotacionAUsar = Quaternion.Euler(-90f, 0f, 0f);
            }
            else if (prefab == timburrPrefab)
            {
            rotacionAUsar = Quaternion.Euler(0f, 90f, 0f);
        }
            if (r <= acum)
            {
                spawnPos.y = 10.5f;
                // Instanciamos este prefab
                Instantiate(prefab, spawnPos, rotacionAUsar);
                Debug.Log($"Power-Up generado (ponderado): {prefab.name}");
                return;
            }
        }
    }


}
