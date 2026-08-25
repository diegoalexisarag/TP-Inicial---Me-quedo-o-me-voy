using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Configuración de Reaparición")]
    [Tooltip("La altura en el eje Y a la que el jugador debe caer para reaparecer.")]
    [SerializeField] private float limiteCaida = -10f;
    
    [Tooltip("El objeto vacío que servirá como punto de reaparición (Opcional).")]
    [SerializeField] private Transform puntoDeReaparicion;

    private CharacterController characterController;
    private Vector3 posicionInicial;

    void Start()
    {
        // Obtenemos el controlador del Starter Asset
        characterController = GetComponent<CharacterController>();
        
        // Guardamos la posición inicial por si no asignas un punto de reaparición manual
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Verificamos si la posición en Y del jugador cae por debajo del límite
        if (transform.position.y < limiteCaida)
        {
            Reaparecer();
        }
    }

    private void Reaparecer()
    {
        // Definimos a dónde irá el jugador. Si hay un punto asignado, va allí. Si no, vuelve al inicio.
        Vector3 posicionDestino = puntoDeReaparicion != null ? puntoDeReaparicion.position : posicionInicial;

        if (characterController != null)
        {
            // Apagamos el CharacterController temporalmente para evitar conflictos con las físicas
            characterController.enabled = false;
            
            // Teletransportamos al jugador
            transform.position = posicionDestino;
            
            // Volvemos a encender el controlador
            characterController.enabled = true;
        }
        else
        {
            // Respaldo por si el objeto no tiene CharacterController
            transform.position = posicionDestino;
        }
    }
}