using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Requiere que este GameObject (el jugador) tenga NetworkObject.
// Requiere que los objetos agarrables tengan: tag "objeto", Rigidbody,
// Collider (no trigger), NetworkObject y NetworkTransform.
public class AgarrarObjeto : NetworkBehaviour
{
    [Tooltip("Posición local (hija del jugador) donde se coloca el objeto agarrado")]
    public Transform myHands;

    private bool canpickup;
    private NetworkObject objetoCercano; // objeto detectado por el trigger, aún no agarrado
    private bool hasItem;
    private NetworkObject objetoActual;  // objeto que tengo agarrado ahora mismo

    void Update()
    {
        // Solo el dueño de este jugador procesa su propio input
        if (!IsOwner) return;

        if (canpickup && !hasItem && objetoCercano != null && Input.GetKeyDown(KeyCode.E))
        {
            SolicitarAgarrarServerRpc(objetoCercano);
        }

        if (hasItem && Input.GetKeyDown(KeyCode.Q))
        {
            SolicitarSoltarServerRpc(objetoActual);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.CompareTag("objeto"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj == null) return; // el objeto agarrable necesita NetworkObject

            canpickup = true;
            objetoCercano = netObj;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        // Solo limpiamos si el que se aleja es justo el que teníamos marcado como cercano
        if (objetoCercano != null && other.gameObject == objetoCercano.gameObject)
        {
            canpickup = false;
            objetoCercano = null;
        }
    }

    // --- SERVIDOR: el servidor es quien decide y aplica el estado real del objeto ---

    [ServerRpc]
    private void SolicitarAgarrarServerRpc(NetworkObjectReference objRef, ServerRpcParams rpcParams = default)
    {
        if (!objRef.TryGet(out NetworkObject netObj)) return;
        if (netObj.transform.parent != null) return; // ya lo tiene otro jugador, ignorar

        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = netObj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Emparentamos el NetworkObject al jugador; Netcode sincroniza esto en todos los clientes
        netObj.TrySetParent(NetworkObject, false);

        AplicarAgarreClientRpc(netObj);
    }

    [ServerRpc]
    private void SolicitarSoltarServerRpc(NetworkObjectReference objRef)
    {
        if (!objRef.TryGet(out NetworkObject netObj)) return;

        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Collider col = netObj.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        netObj.TryRemoveParent(true);

        AplicarSoltadoClientRpc(netObj);
    }

    // --- CLIENTES: solo actualizan su copia local del estado (hasItem, posición en la mano) ---

    [ClientRpc]
    private void AplicarAgarreClientRpc(NetworkObjectReference objRef)
    {
        if (!objRef.TryGet(out NetworkObject netObj)) return;

        if (myHands != null)
        {
            netObj.transform.SetLocalPositionAndRotation(myHands.localPosition, myHands.localRotation);
        }

        if (IsOwner)
        {
            hasItem = true;
            objetoActual = netObj;
            canpickup = false;
            objetoCercano = null;
        }
    }

    [ClientRpc]
    private void AplicarSoltadoClientRpc(NetworkObjectReference objRef)
    {
        if (IsOwner)
        {
            hasItem = false;
            objetoActual = null;
        }
    }
}