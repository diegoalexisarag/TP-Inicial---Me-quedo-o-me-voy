using UnityEngine;
using Unity.Netcode;

public class LineaDeMeta : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; 

        if (other.CompareTag("Player"))
        {
            NetworkObject jugadorNet = other.GetComponent<NetworkObject>();
            if (jugadorNet != null && !MatchManager.Instancia.carreraFinalizada.Value)
            {
                MatchManager.Instancia.DeclararGanador(jugadorNet.OwnerClientId);
            }
        }
    }
}