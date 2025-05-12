using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Tooltip("Velocidad constante de la bola")]
    public float speed = 10f;

    private Rigidbody rb;
    private bool launched = false;
    private float radius;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Sin gravedad, sin drag
        rb.useGravity             = false;
        rb.linearDamping                   = 0f;
        rb.angularDamping            = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;

        // Congelamos SOLO Z, permitiendo rotar
        rb.constraints = RigidbodyConstraints.FreezePositionY;
        
        // Calculamos radio (suponiendo escala uniforme):
        radius = transform.localScale.x * 0.5f;
    }

    void Update()
    {
        if (!launched && Input.GetKeyDown(KeyCode.Space))
        {
            launched = true;
            rb.isKinematic = false;
            // Lanzamiento inicial
            Vector3 dir = new Vector3(1, 0, -1).normalized;
            rb.linearVelocity = dir * speed;
        }
    }

    void FixedUpdate()
    {
        if (!launched) return;

        // 1) Forzamos velocidad constante en XY:
        Vector3 v = rb.linearVelocity;
        v.y = 0;
        rb.linearVelocity = v.normalized * speed;

        // 2) Calculamos giro “sin deslizamiento”:
        //    ω = v / r   (rad/s), en dirección perpendicular al plano XY
        if (v.sqrMagnitude > 0.001f)
        {
            // eje de rotación = n × v̂, con n = (0,0,1)
            Vector3 axis    = Vector3.Cross(Vector3.forward, v.normalized);
            float   omega   = v.magnitude / radius;   // rad/s

            // Asigna rotación
            rb.angularVelocity = axis * omega/5;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // Aquí podrías seguir usando tu lógica de rebote / paddle...
        // Por ejemplo, el rebote en paddle que ya implementaste:
        if (col.gameObject.CompareTag("Paddle"))
        {
            float hitX    = transform.position.x - col.transform.position.x;
            float halfW   = col.collider.bounds.size.x / 2f;
            float nx      = Mathf.Clamp(hitX / halfW, -1f, 1f);
            Vector3 dir   = new Vector3(nx, 0f, -1f).normalized;
            rb.linearVelocity   = dir * speed;
            return;
        }
        else if (col.gameObject.CompareTag("cub"))
        {
            ContactPoint cp   = col.GetContact(0);
            Vector3 incoming  = rb.linearVelocity;
            Vector3 normal    = cp.normal.normalized;
            if (Vector3.Dot(incoming, normal) < 0f)
            {
                Vector3 refl = Vector3.Reflect(incoming, normal);
                refl.y = 0;
                rb.linearVelocity = refl.normalized * speed;
            }
            //rb.linearVelocity = new Vector3(10, 0, 0);
            // 3. Ahora destruir el objeto
            Destroy(col.gameObject.gameObject);

            Debug.Log("¡Colisión con un Cub detectada! Destruyendo...");
        }
        else {
            // Y el resto de colisiones, usando Reflect / tu propia lógica...
            ContactPoint cp   = col.GetContact(0);
            Vector3 incoming  = rb.linearVelocity;
            Vector3 normal    = cp.normal.normalized;
            if (Vector3.Dot(incoming, normal) < 0f)
            {
                Vector3 refl = Vector3.Reflect(incoming, normal);
                refl.y = 0;
                rb.linearVelocity = refl.normalized * speed;
            }
        }
        
    }

    


}
