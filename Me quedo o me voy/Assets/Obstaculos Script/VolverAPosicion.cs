using UnityEngine;
using System.Collections; // Necesario para usar Corrutinas

public class VolverAPosicion : MonoBehaviour
{
    [Tooltip("Tiempo en segundos que tardará en volver")]
    public float tiempoDeEspera = 4.5f;

    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;
    private Rigidbody rb;

    void Start()
    {
        // 1. Guardamos la posición y rotación donde empieza el objeto
        posicionOriginal = transform.position;
        rotacionOriginal = transform.rotation;
        
        rb = GetComponent<Rigidbody>();
    }

    public void Awake()
    {
        StartCoroutine(RutinaVolver());
    }

    // 3. La Corrutina que maneja el tiempo
    private IEnumerator RutinaVolver()
    {
        yield return new WaitForSeconds(tiempoDeEspera);

        transform.position = posicionOriginal;
        transform.rotation = rotacionOriginal;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
            StartCoroutine(RutinaVolver());
        }
    }
}