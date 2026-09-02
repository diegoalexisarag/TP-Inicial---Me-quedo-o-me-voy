using Unity.Netcode;
using UnityEngine;

public class CameraNetworkSetup : MonoBehaviour
{
    private Component virtualCamera;

    private void Awake()
    {
        virtualCamera = GetComponent("CinemachineVirtualCamera");

        if (virtualCamera == null)
        {
            Debug.LogError("No se encontró CinemachineVirtualCamera.");
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Intentar por si el Player ya existe
        AsignarCamara();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        AsignarCamara();
    }

    private void AsignarCamara()
    {
        if (virtualCamera == null)
            return;

        NetworkObject player =
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        if (player == null)
        {
            Debug.Log("Todavía no existe el Player local.");
            return;
        }

        controlar controlador = player.GetComponent<controlar>();

        if (controlador == null)
        {
            Debug.LogError("El Player local no tiene el script controlar.");
            return;
        }

        if (controlador.CinemachineCameraTarget == null)
        {
            Debug.LogError("CinemachineCameraTarget no está asignado.");
            return;
        }

        Transform cameraRoot =
            controlador.CinemachineCameraTarget.transform;

        // CinemachineVirtualCamera.Follow
        virtualCamera.GetType()
            .GetProperty("Follow")
            ?.SetValue(virtualCamera, cameraRoot);

        // CinemachineVirtualCamera.LookAt
        virtualCamera.GetType()
            .GetProperty("LookAt")
            ?.SetValue(virtualCamera, cameraRoot);

        Debug.Log(
            "Cámara asignada al Player local: " + player.name
        );
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
