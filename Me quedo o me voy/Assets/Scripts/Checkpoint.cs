using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Arrastra aquí un objeto vacío que esté tocando exactamente el suelo")]
    [SerializeField] public Transform puntoSeguroDeReaparicion;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // En lugar de pasar 'this.transform', pasamos las coordenadas precisas del punto seguro
            other.GetComponent<PlayerRespawn>().puntoDeReaparicion = puntoSeguroDeReaparicion;
            
            // Opcional: Podrías añadir un Debug.Log aquí para confirmar que se guardó
            Debug.Log("¡Checkpoint se ha guardado en: " + puntoSeguroDeReaparicion.name + "!");
        }
    }
}
