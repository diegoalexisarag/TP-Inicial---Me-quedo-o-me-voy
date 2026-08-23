using UnityEngine;

public class MovimientoPorPuntos : MonoBehaviour
{
    [Tooltip("El primer punto de la ruta (Ej: Posición inicial)")]
    public Transform puntoA;
    
    [Tooltip("El segundo punto de la ruta (Ej: Posición final)")]
    public Transform puntoB;
    
    [Tooltip("Velocidad de desplazamiento de la sierra")]
    public float speed = 2f;

    // Almacena hacia dónde se está moviendo actualmente
    private Transform objetivoActual;

    void Start()
    {
        // Al iniciar, le decimos que se dirija al punto B
        objetivoActual = puntoB;
    }

    void Update()
    {
        // 1. Movemos el objeto paso a paso hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, objetivoActual.position, speed * Time.deltaTime);

        // 2. Comprobamos si ya llegó (con un pequeño margen de error para evitar bugs de precisión)
        if (Vector3.Distance(transform.position, objetivoActual.position) < 0.1f)
        {
            // 3. Si llegó a B, cambiamos el objetivo a A, y viceversa
            if (objetivoActual == puntoA)
            {
                objetivoActual = puntoB;
            }
            else
            {
                objetivoActual = puntoA;
            }
        }
    }
}
