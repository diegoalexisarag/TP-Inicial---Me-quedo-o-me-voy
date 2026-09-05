using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class GestorClima : NetworkBehaviour
{
    public enum EstadoClima { Ninguno = 0, Lluvia = 1, Nieve = 2 }

    [Header("Referencias a los efectos")]
    [Tooltip("El GameObject 'Rain' (hijo de Particles)")]
    public GameObject lluvia;

    [Tooltip("El GameObject 'ParticlesSnow'")]
    public GameObject nieve;

    [Header("Probabilidades (pesos relativos, no hace falta que sumen 1)")]
    [Tooltip("Peso de que no haya clima")]
    public float pesoNinguno = 1f;
    [Tooltip("Peso de que llueva")]
    public float pesoLluvia = 1f;
    [Tooltip("Peso de que nieve")]
    public float pesoNieve = 1f;

    private NetworkVariable<int> estadoClima = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            estadoClima.Value = (int)SortearClima();
        }

        AplicarEstadoClima((EstadoClima)estadoClima.Value);

        estadoClima.OnValueChanged += OnClimaCambio;
    }

    public override void OnNetworkDespawn()
    {
        estadoClima.OnValueChanged -= OnClimaCambio;
    }

    private void OnClimaCambio(int valorAnterior, int valorNuevo)
    {
        AplicarEstadoClima((EstadoClima)valorNuevo);
    }

    private EstadoClima SortearClima()
    {
        float total = pesoNinguno + pesoLluvia + pesoNieve;
        if (total <= 0f) return EstadoClima.Ninguno;

        float roll = Random.value * total;

        if (roll < pesoNinguno) return EstadoClima.Ninguno;
        roll -= pesoNinguno;

        if (roll < pesoLluvia) return EstadoClima.Lluvia;

        return EstadoClima.Nieve;
    }

    private void AplicarEstadoClima(EstadoClima estado)
    {
        if (lluvia != null) lluvia.SetActive(estado == EstadoClima.Lluvia);
        if (nieve != null) nieve.SetActive(estado == EstadoClima.Nieve);
    }
}