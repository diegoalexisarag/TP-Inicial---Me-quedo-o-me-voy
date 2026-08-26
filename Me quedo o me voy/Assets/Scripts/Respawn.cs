using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Configuración de Reaparición")]
    [SerializeField] private float limiteCaida = -10f;
    [SerializeField] public Transform puntoDeReaparicion;

    private CharacterController characterController;
    private Vector3 posicionInicial;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (transform.position.y < limiteCaida)
        {
            Reaparecer();
        }
    }

    public void Reaparecer()
    {
        Vector3 posicionDestino = puntoDeReaparicion != null ? puntoDeReaparicion.position : posicionInicial;

        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = posicionDestino;
            characterController.enabled = true;
        }
        else
        {
            transform.position = posicionDestino;
        }
    }

   private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detectado con: " + other.gameObject.name + " | Tag: " + other.tag);
        if (other.CompareTag("obstaculo-letal"))
        {
            Reaparecer();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("Choque detectado con: " + hit.gameObject.name + " | Tag: " + hit.gameObject.tag);
        if (hit.gameObject.CompareTag("obstaculo-letal"))
        {
            Reaparecer();
        }
    }
}