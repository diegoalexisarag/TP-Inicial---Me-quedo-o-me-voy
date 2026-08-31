using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private ISession currentSession;

    public string JoinCode { get; private set; }

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InicializarServicios();
    }

    private async System.Threading.Tasks.Task InicializarServicios()
    {
        try
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Unity Services inicializados.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error inicializando Unity Services: " + e);
        }
    }

    public async void CrearPartida()
    {
        try
        {
            Debug.Log("Creando partida...");

            var options = new SessionOptions
            {
                MaxPlayers = 4
            }.WithRelayNetwork();

            currentSession =
                await MultiplayerService.Instance.CreateSessionAsync(options);

            JoinCode = currentSession.Code;

            Debug.Log("=================================");
            Debug.Log("PARTIDA CREADA");
            Debug.Log("CÓDIGO: " + JoinCode);
            Debug.Log("=================================");

            await System.Threading.Tasks.Task.Yield();

            NetworkManager.Singleton.SceneManager.LoadScene("EscenarioInicial", LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Debug.LogError("Error creando partida: " + e);
        }
    }

    public async void UnirsePartida(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                Debug.LogError("El código está vacío.");
                return;
            }

            codigo = codigo.Trim().ToUpper();

            Debug.Log("Intentando unirse a: " + codigo);

            currentSession =
                await MultiplayerService.Instance.JoinSessionByCodeAsync(codigo);
            JoinCode = codigo;
            Debug.Log("=================================");
            Debug.Log("PARTIDA ENCONTRADA");
            Debug.Log("CÓDIGO: " + codigo);
            Debug.Log("=================================");
        }
        catch (Exception e)
        {
            Debug.LogError("Error uniéndose a la partida: " + e);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.LogError("=== CLIENTE DESCONECTADO === ID: " + clientId);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }
}