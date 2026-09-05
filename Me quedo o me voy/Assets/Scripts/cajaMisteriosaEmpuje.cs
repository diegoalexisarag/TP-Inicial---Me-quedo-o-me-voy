using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
public class cajaMisteriosaEmpuje : NetworkBehaviour
{
    [Header("Configuración del Poder de Empuje")]
    [Tooltip("Cuánto dura el efecto, en segundos")]
    public float duracionBoost = 8f;

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
        if (!IsServer || yaActivada) return;

        controlar jugador = other.GetComponent<controlar>();
        if (jugador == null) return;

        NetworkObject jugadorNetObj = jugador.GetComponent<NetworkObject>();
        if (jugadorNetObj == null) return;

        yaActivada = true;

        ClientRpcParams paramsSoloDueño = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { jugadorNetObj.OwnerClientId }
            }
        };
        AplicarEmpujeClientRpc(jugadorNetObj, duracionBoost, paramsSoloDueño);

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
    private void AplicarEmpujeClientRpc(NetworkObjectReference jugadorRef, float tiempo, ClientRpcParams rpcParams = default)
    {
        if (jugadorRef.TryGet(out NetworkObject jugadorNetObj))
        {
            controlar jugador = jugadorNetObj.GetComponent<controlar>();
            jugador?.ActivarPoderEmpuje(tiempo);
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