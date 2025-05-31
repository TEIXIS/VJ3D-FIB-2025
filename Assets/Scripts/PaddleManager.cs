using System.Collections;
using UnityEngine;

public class PaddleManager : MonoBehaviour
{
    [Header("Prefabs de Paleta+Pokémon")]
    [Tooltip("Prefab donde la paleta está asociada a Gurdurr (todos los hijos en 0,0,0)")]
    public GameObject prefabPaddleGurdurr;
    [Tooltip("Prefab donde la paleta está asociada a Timburr (todos los hijos en 0,0,0)")]
    public GameObject prefabPaddleTimburr;
    [Tooltip("Prefab donde la paleta está asociada a Conkeldurr (todos los hijos en 0,0,0)")]
    public GameObject prefabPaddleConkeldurr;

    [Header("Duración de cada transformación (segundos)")]
    public float transformDuration = 30f;

    // Instancia actualmente activa (PaddleGurdurr, PaddleTimburr o PaddleConkeldurr)
    private GameObject activePaddleInstance;

    // Para controlar la coroutine en curso (detener si llega otro power-up)
    private Coroutine swapCoroutine;

    void Awake()
    {
        // Al iniciar la escena, instanciamos la paleta base (Gurdurr)
        SpawnInitialPaddle();
    }

    /// <summary>
    /// Instancia el prefab de Gurdurr exactamente en la posición de PaddleRoot.
    /// </summary>
    private void SpawnInitialPaddle()
    {
        // Si ya existe una instancia previa, la destruimos
        if (activePaddleInstance != null)
            Destroy(activePaddleInstance);

        // 1) Instanciamos Gurdurr en la posición world de PaddleRoot (transform.position)
        activePaddleInstance = Instantiate(
            prefabPaddleGurdurr,
            transform.position,
            transform.rotation
        );

        // 2) Parentearlo a PaddleRoot sin conservar la posición world antigua:
        //    con "worldPositionStays: false" Unity reajusta la posición para que sea local.
        activePaddleInstance.transform.SetParent(transform, worldPositionStays: false);

        // 3) Fijamos localPosition = (0,0,0) para que coincida exactamente con el padre
        activePaddleInstance.transform.localPosition = Vector3.zero;
        activePaddleInstance.transform.localRotation = Quaternion.Euler(0,180,0);
    }

    /// <summary>
    /// Llamar desde un power-up para transformar la paleta en Timburr durante 'transformDuration' segundos.
    /// </summary>
    public void ActivateTimburrMode()
    {
        if (swapCoroutine != null)
            StopCoroutine(swapCoroutine);

        swapCoroutine = StartCoroutine(SwapPaddle(prefabPaddleTimburr));
    }

    /// <summary>
    /// Llamar desde un power-up para transformar la paleta en Conkeldurr durante 'transformDuration' segundos.
    /// </summary>
    public void ActivateConkeldurrMode()
    {
        if (swapCoroutine != null)
            StopCoroutine(swapCoroutine);

        swapCoroutine = StartCoroutine(SwapPaddle(prefabPaddleConkeldurr));
    }

    /// <summary>
    /// Coroutine genérica que reemplaza el modelo actual por 'newPrefab' durante 'transformDuration',
    /// y al acabar vuelve a Gurdurr (usando SpawnInitialPaddle()).
    /// </summary>
    private IEnumerator SwapPaddle(GameObject newPrefab)
    {
        // 1) Destruye la instancia anterior (Gurdurr, Timburr o Conkeldurr)
        if (activePaddleInstance != null)
            Destroy(activePaddleInstance);

        // 2) Esperamos un frame para que Unity procese la destrucción
        yield return null;

        // 3) Instanciamos el nuevo prefab exactamente en transform.position
        activePaddleInstance = Instantiate(
            newPrefab,
            transform.position,
            transform.rotation
        );

        // 4) Parentearlo a PaddleRoot sin conservar la posición world (para fijar local = 0)
        activePaddleInstance.transform.SetParent(transform, worldPositionStays: false);

        // 5) Fijamos localPosition/localRotation a cero (para que “caiga” exactamente en el padre)
        activePaddleInstance.transform.localPosition = Vector3.zero;
        activePaddleInstance.transform.localRotation = Quaternion.Euler(0,180,0);

        // 6) Esperamos la duración del power-up
        yield return new WaitForSeconds(transformDuration);

        // 7) Destruimos la instancia de Timburr/Conkeldurr y volvemos a Gurdurr
        if (activePaddleInstance != null)
            Destroy(activePaddleInstance);

        yield return null; // espera un frame

        // 8) Re-usamos SpawnInitialPaddle() para instanciar Gurdurr de nuevo
        SpawnInitialPaddle();

        swapCoroutine = null;
    }
}
