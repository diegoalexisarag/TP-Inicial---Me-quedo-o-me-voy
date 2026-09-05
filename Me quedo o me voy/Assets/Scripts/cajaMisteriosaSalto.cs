using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
public class cajaMisteriosaSalto : NetworkBehaviour
{
    [Header("Configuración del Super Salto")]
    [Tooltip("Cuánto se multiplica la altura de salto (2 = el doble)")]
    public float multiplicadorSalto = 2f;

    [Tooltip("Cuánto dura el efecto, en segundos")]
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
        AplicarSaltoClientRpc(jugadorNetObj, multiplicadorSalto, duracionBoost, paramsSoloDueño);

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
}