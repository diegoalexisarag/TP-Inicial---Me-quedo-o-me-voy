using UnityEngine;
using Unity.Netcode;

// Misma lógica que CajaMisteriosa (velocidad), pero llamando a
// ActivarSuperSalto(multiplicador, tiempo), que ya existe en controlar.cs.
// No se modifica controlar.cs para nada.
[RequireComponent(typeof(Collider))]
public class cajaMisteriosaSalto : NetworkBehaviour
{
    [Header("Configuración del Super Salto")]
    [Tooltip("Cuánto se multiplica la altura de salto (2 = el doble)")]
    public float multiplicadorSalto = 2f;

    [Tooltip("Cuánto dura el efecto, en segundos")]
    public float duracionBoost = 5f;

    [Header("Efectos (opcional)")]
    [Tooltip("Partícula o efecto que se instancia al recogerla (se ve en todos los clientes)")]
    public GameObject efectoAlRecoger;

    [Tooltip("Sonido al recogerla")]
    public AudioClip sonidoAlRecoger;

    private bool yaActivada = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo el servidor decide si la caja fue tocada
        if (!IsServer || yaActivada) return;

        controlar jugador = other.GetComponent<controlar>();
        if (jugador == null) return;

        NetworkObject jugadorNetObj = jugador.GetComponent<NetworkObject>();
        if (jugadorNetObj == null) return;

        yaActivada = true;

        // El salto (jumpHeight) también se lee solo localmente en el dueño,
        // así que el RPC va dirigido únicamente a él.
        ClientRpcParams paramsSoloDueño = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { jugadorNetObj.OwnerClientId }
            }
        };
        AplicarSaltoClientRpc(jugadorNetObj, multiplicadorSalto, duracionBoost, paramsSoloDueño);

        MostrarEfectoClientRpc();

        NetworkObject miNetObj = GetComponent<NetworkObject>();
        if (miNetObj != null)
        {
            miNetObj.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void AplicarSaltoClientRpc(NetworkObjectReference jugadorRef, float multiplicador, float tiempo, ClientRpcParams rpcParams = default)
    {
        if (jugadorRef.TryGet(out NetworkObject jugadorNetObj))
        {
            controlar jugador = jugadorNetObj.GetComponent<controlar>();
            jugador?.ActivarSuperSalto(multiplicador, tiempo);
        }
    }

    [ClientRpc]
    private void MostrarEfectoClientRpc()
    {
        if (efectoAlRecoger != null)
            Instantiate(efectoAlRecoger, transform.position, Quaternion.identity);

        if (sonidoAlRecoger != null)
            AudioSource.PlayClipAtPoint(sonidoAlRecoger, transform.position);
    }
}