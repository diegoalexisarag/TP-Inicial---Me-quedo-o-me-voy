using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class ControladorLluvia : NetworkBehaviour
{
    [Header("Configuración")]
    [Tooltip("Probabilidad de que llueva al iniciar la partida (0 a 1). 0.5 = 50%")]
    [Range(0f, 1f)]
    public float probabilidadDeLluvia = 0.5f;

    [Tooltip("El GameObject 'Rain' que tiene el Particle System")]
    public GameObject lluvia;

    private NetworkVariable<bool> estaLloviendo = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            estaLloviendo.Value = Random.value < probabilidadDeLluvia;
        }

        AplicarEstadoLluvia(estaLloviendo.Value);

        Debug.Log("Lluvia decidida: " + estaLloviendo.Value);

        estaLloviendo.OnValueChanged += OnLluviaCambio;
    }

    public override void OnNetworkDespawn()
    {
        estaLloviendo.OnValueChanged -= OnLluviaCambio;
    }

    private void OnLluviaCambio(bool valorAnterior, bool valorNuevo)
    {
        AplicarEstadoLluvia(valorNuevo);
    }

    private void AplicarEstadoLluvia(bool activa)
    {
        if (lluvia != null)
        {
            lluvia.SetActive(activa);
        }
    }
}