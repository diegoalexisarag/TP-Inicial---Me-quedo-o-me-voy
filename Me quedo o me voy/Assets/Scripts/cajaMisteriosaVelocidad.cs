using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
public class cajaMisteriosaVelocidad : NetworkBehaviour
{
    [Header("Configuración del Boost")]
    [Tooltip("Cuánto se multiplica la velocidad (2 = el doble)")]
    public float multiplicadorVelocidad = 2f;

    [Tooltip("Cuánto dura el boost, en segundos")]
    public float duracionBoost = 5f;

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
        AplicarBoostClientRpc(jugadorNetObj, multiplicadorVelocidad, duracionBoost, paramsSoloDueño);

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
    private void AplicarBoostClientRpc(NetworkObjectReference jugadorRef, float multiplicador, float tiempo, ClientRpcParams rpcParams = default)
    {
        if (jugadorRef.TryGet(out NetworkObject jugadorNetObj))
        {
            controlar jugador = jugadorNetObj.GetComponent<controlar>();
            jugador?.ActivarSpeedBoost(multiplicador, tiempo);
        }
    }
}