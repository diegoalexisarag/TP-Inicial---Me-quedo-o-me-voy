using UnityEngine;
using Unity.Netcode;

// La caja necesita un Collider marcado como "Is Trigger" y, si tu escena es
// multijugador (como indica "controlar.cs" con Unity Netcode), también un
// componente NetworkObject para que la desaparición se sincronice en todos
// los clientes.
[RequireComponent(typeof(Collider))]
public class cajaMisteriosaVelocidad : NetworkBehaviour
{
    [Header("Configuración del Boost")]
    [Tooltip("Cuánto se multiplica la velocidad (2 = el doble)")]
    public float multiplicadorVelocidad = 2f;

    [Tooltip("Cuánto dura el boost, en segundos")]
    public float duracionBoost = 5f;

    [Header("Efectos (opcional)")]
    [Tooltip("Partícula o efecto que se instancia al recogerla (se ve en todos los clientes)")]
    public GameObject efectoAlRecoger;

    [Tooltip("Sonido al recogerla")]
    public AudioClip sonidoAlRecoger;

    // Evita que dos jugadores la activen a la vez por un frame de más
    private bool yaActivada = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo el servidor decide si la caja fue tocada, así evitamos que
        // cada cliente la procese por su cuenta y se desincronice.
        if (!IsServer || yaActivada) return;

        controlar jugador = other.GetComponent<controlar>();
        if (jugador == null) return;

        NetworkObject jugadorNetObj = jugador.GetComponent<NetworkObject>();
        if (jugadorNetObj == null) return;

        yaActivada = true;

        // El boost de velocidad solo lo aplica localmente el dueño de ese
        // jugador (walkSpeed/runSpeed se leen en el Update() del propio
        // cliente), así que se lo pedimos únicamente a él con un RPC dirigido.
        ClientRpcParams paramsSoloDueño = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { jugadorNetObj.OwnerClientId }
            }
        };
        AplicarBoostClientRpc(jugadorNetObj, multiplicadorVelocidad, duracionBoost, paramsSoloDueño);

        // Efecto/sonido para todo el mundo, antes de que la caja desaparezca
        MostrarEfectoClientRpc();

        // Despawn en el servidor: destruye la caja en TODOS los clientes
        NetworkObject miNetObj = GetComponent<NetworkObject>();
        if (miNetObj != null)
        {
            miNetObj.Despawn(true); // true = también destruye el GameObject
        }
        else
        {
            // Fallback por si la caja no tiene NetworkObject (juego no-networked)
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void AplicarBoostClientRpc(NetworkObjectReference jugadorRef, float multiplicador, float tiempo, ClientRpcParams rpcParams = default)
    {
        if (jugadorRef.TryGet(out NetworkObject jugadorNetObj))
        {
            controlar jugador = jugadorNetObj.GetComponent<controlar>();
            jugador?.ActivarSpeedBoost(multiplicador, tiempo);
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