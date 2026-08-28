using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerRespawnAndCheckpoint : MonoBehaviour
{
    [Header("Configuración de Caída")]
    public float dead;
    public List<GameObject> checkPoints;
    private Vector3 puntoDeReaparicion;

    void Start()
    {
        puntoDeReaparicion = transform.position;
    }

    void Update()
    {
        // Si la posición en Y del jugador es menor al límite negativo
        if (transform.position.y < -dead)
        {
            Reaparecer();
        }
    }

    void Reaparecer()
    {
        // Movemos al jugador al punto guardado
        transform.position = puntoDeReaparicion;

    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo actualizamos el checkpoint si el objeto tiene la etiqueta "Checkpoint"
        if (other.CompareTag("Point"))
        {
            puntoDeReaparicion = other.transform.position;
            Destroy(other.gameObject);
        }
    }
}
