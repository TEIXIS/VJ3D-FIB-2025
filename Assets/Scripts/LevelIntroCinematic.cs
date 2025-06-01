using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LevelIntroCinematic : MonoBehaviour
{
    [Header("Punto alrededor del cual girar")]
    [Tooltip("Un empty GameObject centrado en la pared de ladrillos o en el foco de atención del nivel")]
    public Transform pivotPoint;

    [Header("Duración total de la cinemática (en segundos)")]
    public float duration = 5f;

    // Guardamos posición y rotación originales para restaurar al final
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool hasPlayed = false;

    void Start()
    {
        // 1) Asegurarnos de deshabilitar controles desde el inicio
        GameState.allowInput = false;

        // 2) Guardamos la posición/rotación inicial
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // 3) Si pivotPoint no está asignado, usamos el origen (0,0,0)
        if (pivotPoint == null)
        {
            GameObject go = new GameObject("PivotTemp");
            go.transform.position = Vector3.zero;
            pivotPoint = go.transform;
        }

        // 4) Iniciamos la corutina que hará la vuelta de cámara
        StartCoroutine(PlayCinematic());
    }

    private IEnumerator PlayCinematic()
    {
        // Para evitar que se ejecute dos veces si recargan escena
        if (hasPlayed) yield break;
        hasPlayed = true;

        float elapsed = 0f;

        // Distancia desde el pivot a la cámara, para mantener radio constante
        Vector3 offset = originalPosition - pivotPoint.position;
        float radius = offset.magnitude;

        while (elapsed < duration)
        {
            // Calculamos cuántos grados avanzar esta frame
            float t = elapsed / duration; // va de 0 a 1
            float angle = Mathf.Lerp(0f, 360f, t);

            // Rotar la cámara alrededor del pivot en Y
            //   1) Construimos una rotación que gire alrededor de Y
            Quaternion rotY = Quaternion.Euler(0f, angle, 0f);
            //   2) Aplicamos esa rotación sobre el vector offset original
            Vector3 newPos = pivotPoint.position + rotY * offset;

            transform.position = newPos;
            // Miramos siempre hacia el pivot
            transform.LookAt(pivotPoint);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Al terminar, restauramos exactamente la posición/rotación original
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Dar control al jugador
        GameState.allowInput = true;
    }
}
